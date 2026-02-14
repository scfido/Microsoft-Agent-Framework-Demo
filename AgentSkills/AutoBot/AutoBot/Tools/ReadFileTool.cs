using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 读取文件工具，支持 workspace 和 skill 作用域。
/// </summary>
public sealed class ReadFileTool
{
    private readonly RuntimeOptions _options;
    private readonly SkillsState _state;

    /// <summary>
    /// 初始化 ReadFileTool 实例。
    /// </summary>
    public ReadFileTool(RuntimeOptions options, SkillsState state)
    {
        _options = options;
        _state = state;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options, SkillsState state)
    {
        var tool = new ReadFileTool(options, state);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "read_file");
    }

    /// <summary>
    /// 读取文件内容。
    /// </summary>
    [Description("读取文件内容")]
    public async Task<string> ExecuteAsync(
        [Description("文件相对路径")] string filePath,
        [Description("作用域：workspace（默认）或 skill")] string scope = "workspace",
        [Description("当 scope=skill 时必填的技能名称")] string? skillName = null,
        [Description("起始行号（从 1 开始）")] int? offset = null,
        [Description("读取行数限制")] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解析作用域
            var toolScope = Enum.Parse<ToolScope>(scope, ignoreCase: true);
            var baseDir = ResolveBaseDirectory(toolScope, skillName);

            // 安全解析路径
            var safePath = PathSecurity.ResolveSafePath(baseDir, filePath);
            if (safePath == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "路径遍历尝试被拒绝",
                    file_path = filePath
                });
            }

            if (!File.Exists(safePath))
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "文件不存在",
                    file_path = filePath
                });
            }

            // 检查文件大小
            var fileInfo = new FileInfo(safePath);
            if (fileInfo.Length > _options.Tools.MaxFileSizeBytes)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"文件大小 ({fileInfo.Length} 字节) 超过限制 ({_options.Tools.MaxFileSizeBytes} 字节)",
                    file_path = filePath
                });
            }

            // 读取文件内容
            var content = await File.ReadAllTextAsync(safePath, cancellationToken);

            // 应用偏移和限制
            if (offset.HasValue || limit.HasValue)
            {
                var lines = content.Split('\n');
                var startLine = Math.Max(0, (offset ?? 1) - 1);
                var count = limit ?? (lines.Length - startLine);
                content = string.Join('\n', lines.Skip(startLine).Take(count));
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                file_path = filePath,
                size_bytes = fileInfo.Length,
                content
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                file_path = filePath
            });
        }
    }

    private string ResolveBaseDirectory(ToolScope scope, string? skillName)
    {
        return scope switch
        {
            ToolScope.Workspace => _options.WorkingDirectory,
            ToolScope.Skill => ResolveSkillDirectory(skillName ?? throw new ArgumentException("skillName 在 scope=skill 时必填")),
            _ => throw new ArgumentException($"不支持的作用域: {scope}")
        };
    }

    private string ResolveSkillDirectory(string skillName)
    {
        var skill = _state.GetSkill(skillName);
        if (skill == null)
        {
            throw new ArgumentException($"技能 '{skillName}' 未找到");
        }
        return skill.Path;
    }
}
