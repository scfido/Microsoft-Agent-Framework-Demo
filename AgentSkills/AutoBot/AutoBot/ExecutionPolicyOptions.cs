using AutoBot.SkillEngine.Execution;

namespace AutoBot;

/// <summary>
/// 执行策略配置：白名单、风险评估、人工确认。
/// </summary>
public sealed class ExecutionPolicyOptions
{
    /// <summary>
    /// 获取或设置命令白名单。
    /// 若列表非空，则仅允许白名单中的命令执行（严格模式）。
    /// 若列表为空，允许执行所有命令并进行风险评估（风险评估模式）。
    /// 默认为空列表。
    /// </summary>
    public IList<string> AllowedCommands { get; set; } = new List<string>();

    /// <summary>
    /// 获取或设置危险命令模式列表，用于风险分级。
    /// 匹配这些模式的命令将被评估为 High 或 Critical 风险。
    /// </summary>
    public IList<string> RiskyCommandPatterns { get; set; } = new List<string>
    {
        "rm -rf", "format", "shutdown", "reboot", "del /f", "rmdir /s"
    };

    /// <summary>
    /// 获取或设置安全命令模式列表，用于风险分级。
    /// 匹配这些模式的命令将被评估为 Safe 风险。
    /// </summary>
    public IList<string> SafeCommandPatterns { get; set; } = new List<string>
    {
        "ls", "dir", "echo", "cat", "type", "pwd", "cd", "git status"
    };

    /// <summary>
    /// 获取或设置是否自动批准只读操作。
    /// 默认为 true（自动执行，不阻塞）。
    /// </summary>
    public bool AutoApproveReadOperations { get; set; } = true;

    /// <summary>
    /// 获取或设置是否自动批准安全命令。
    /// 默认为 true（自动执行）。
    /// </summary>
    public bool AutoApproveSafeCommands { get; set; } = true;

    /// <summary>
    /// 获取或设置自定义人工确认处理器。
    /// 若为 null，则不触发确认（自动批准所有操作）。
    /// 默认为 null。
    /// </summary>
    public IHumanConfirmation? HumanConfirmation { get; set; }
}
