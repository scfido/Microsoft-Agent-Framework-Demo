using AutoBot.SkillEngine;

namespace AutoBot;

/// <summary>
/// 运行时状态容器，持有已加载的技能集合。
/// </summary>
public sealed class SkillsState
{
    /// <summary>
    /// 获取已加载的技能集合。
    /// </summary>
    public IReadOnlyList<SkillMetadata> Skills { get; init; } = [];

    /// <summary>
    /// 获取所有技能。
    /// </summary>
    public IReadOnlyList<SkillMetadata> AllSkills => Skills;

    /// <summary>
    /// 按名称获取技能（大小写不敏感）。
    /// </summary>
    /// <param name="name">技能名称。</param>
    /// <returns>技能元数据，若未找到则返回 null。</returns>
    public SkillMetadata? GetSkill(string name)
    {
        return Skills.FirstOrDefault(
            s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
