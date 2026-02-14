using AutoBot.SkillEngine.Execution;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 执行命令工具，实现白名单优先 + 风险分级策略。
/// </summary>
public sealed class RunCommandTool
{
    private readonly RuntimeOptions _options;

    /// <summary>
    /// 初始化 RunCommandTool 实例。
    /// </summary>
    public RunCommandTool(RuntimeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options)
    {
        var tool = new RunCommandTool(options);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "run_command");
    }

    /// <summary>
    /// 执行命令。
    /// </summary>
    [Description("执行 shell 命令")]
    public async Task<string> ExecuteAsync(
        [Description("要执行的命令")] string command,
        [Description("工作目录（可选，默认为配置的工作目录）")] string? workingDirectory = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = _options.ExecutionPolicy;

            // 策略 1：白名单优先（严格模式）
            if (policy.AllowedCommands.Count > 0)
            {
                var commandBase = ExtractCommandBase(command);
                if (!policy.AllowedCommands.Contains(commandBase, StringComparer.OrdinalIgnoreCase))
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "命令不在白名单中",
                        command,
                        allowed_commands = policy.AllowedCommands,
                        risk_level = "rejected"
                    });
                }
            }

            // 策略 2：风险评估模式
            var riskLevel = AssessRiskLevel(command, policy);

            // 策略 3：可选人工确认
            if (policy.HumanConfirmation != null && !ShouldAutoApprove(riskLevel, policy))
            {
                var confirmed = await policy.HumanConfirmation.ConfirmAsync(
                    "run_command",
                    $"执行命令: {command}",
                    riskLevel,
                    cancellationToken);

                if (!confirmed)
                {
                    return JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "用户拒绝执行",
                        command,
                        risk_level = riskLevel.ToString()
                    });
                }
            }

            // 执行命令
            var workDir = workingDirectory ?? _options.WorkingDirectory;
            var result = await ExecuteCommandInternal(command, workDir, cancellationToken);

            return JsonSerializer.Serialize(new
            {
                success = result.ExitCode == 0,
                command,
                exit_code = result.ExitCode,
                output = result.Output,
                error = result.Error,
                risk_level = riskLevel.ToString(),
                risk_assessment = GetRiskAssessment(riskLevel)
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                command
            });
        }
    }

    /// <summary>
    /// 评估命令的风险等级。
    /// </summary>
    private RiskLevel AssessRiskLevel(string command, ExecutionPolicyOptions policy)
    {
        var lowerCommand = command.ToLowerInvariant();

        // Critical: 极危险命令
        foreach (var pattern in policy.RiskyCommandPatterns)
        {
            if (lowerCommand.Contains(pattern.ToLowerInvariant()))
            {
                return RiskLevel.Critical;
            }
        }

        // Safe: 安全命令
        foreach (var pattern in policy.SafeCommandPatterns)
        {
            if (lowerCommand.StartsWith(pattern.ToLowerInvariant()))
            {
                return RiskLevel.Safe;
            }
        }

        // Medium: 默认中等风险
        return RiskLevel.Medium;
    }

    /// <summary>
    /// 判断是否应该自动批准。
    /// </summary>
    private bool ShouldAutoApprove(RiskLevel riskLevel, ExecutionPolicyOptions policy)
    {
        return riskLevel switch
        {
            RiskLevel.Safe => policy.AutoApproveSafeCommands,
            _ => false
        };
    }

    /// <summary>
    /// 获取风险评估描述。
    /// </summary>
    private string GetRiskAssessment(RiskLevel riskLevel)
    {
        return riskLevel switch
        {
            RiskLevel.Safe => "安全操作，无风险",
            RiskLevel.Medium => "中等风险，可能修改系统状态",
            RiskLevel.High => "高风险操作，可能造成数据丢失",
            RiskLevel.Critical => "极高风险，可能造成不可逆损害",
            _ => "未知风险"
        };
    }

    /// <summary>
    /// 提取命令的基本部分（命令名）。
    /// </summary>
    private string ExtractCommandBase(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : command;
    }

    /// <summary>
    /// 内部执行命令。
    /// </summary>
    private async Task<CommandResult> ExecuteCommandInternal(
        string command,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var isWindows = OperatingSystem.IsWindows();
        var shellPath = isWindows ? "cmd.exe" : "/bin/bash";
        var shellArgs = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = shellPath,
            Arguments = shellArgs,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 应用超时
        var timeoutMs = _options.Tools.CommandTimeoutSeconds * 1000;
        var completed = await Task.Run(() => process.WaitForExit(timeoutMs), cancellationToken);

        if (!completed)
        {
            process.Kill(entireProcessTree: true);
            return new CommandResult
            {
                ExitCode = -1,
                Output = outputBuilder.ToString(),
                Error = $"命令执行超时（{_options.Tools.CommandTimeoutSeconds} 秒）"
            };
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        // 限制输出大小
        if (output.Length > _options.Tools.MaxOutputSizeBytes)
        {
            output = output.Substring(0, _options.Tools.MaxOutputSizeBytes) + "\n...(输出已截断)";
        }

        if (error.Length > _options.Tools.MaxOutputSizeBytes)
        {
            error = error.Substring(0, _options.Tools.MaxOutputSizeBytes) + "\n...(错误输出已截断)";
        }

        return new CommandResult
        {
            ExitCode = process.ExitCode,
            Output = output,
            Error = error
        };
    }

    /// <summary>
    /// 命令执行结果。
    /// </summary>
    private sealed class CommandResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
