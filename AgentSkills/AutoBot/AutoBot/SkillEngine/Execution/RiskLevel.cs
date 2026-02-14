namespace AutoBot.SkillEngine.Execution;

/// <summary>
/// 操作风险等级。
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// 安全操作（只读操作，无风险）。
    /// </summary>
    Safe = 0,

    /// <summary>
    /// 中等风险操作（写入文件、一般命令）。
    /// </summary>
    Medium = 1,

    /// <summary>
    /// 高风险操作（删除文件、危险命令）。
    /// </summary>
    High = 2,

    /// <summary>
    /// 极高风险操作（rm -rf、format、shutdown 等）。
    /// </summary>
    Critical = 3
}
