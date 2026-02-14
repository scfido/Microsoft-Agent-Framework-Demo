namespace AutoBot.SkillEngine;

/// <summary>
/// 指示技能的来源位置。
/// </summary>
public enum SkillSource
{
    /// <summary>
    /// 存储在用户主目录中的用户级技能。
    /// 路径：~/.maf/{agent-name}/skills/{skill-name}/
    /// </summary>
    User,

    /// <summary>
    /// 存储在项目 .maf 目录中的项目级技能。
    /// 路径：{project-root}/.maf/skills/{skill-name}/
    /// </summary>
    Project
}
