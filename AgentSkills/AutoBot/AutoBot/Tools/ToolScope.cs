namespace AutoBot.Tools;

/// <summary>
/// 工具作用域，指定操作的根目录。
/// </summary>
public enum ToolScope
{
    /// <summary>
    /// 工作目录作用域（默认）。
    /// 所有操作限制在 WorkingDirectory 内。
    /// </summary>
    Workspace,

    /// <summary>
    /// 技能目录作用域。
    /// 所有操作限制在指定技能的目录内。
    /// </summary>
    Skill
}
