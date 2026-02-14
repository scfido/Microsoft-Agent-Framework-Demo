using AutoBot.SkillEngine;

namespace AutoBot;

/// <summary>
/// 本地运行时的状态，包含已加载的技能信息。
/// </summary>
public sealed class SkillsState
{
    /// <summary>
    /// 获取或设置用户级技能集合。
    /// </summary>
    public IReadOnlyList<SkillMetadata> UserSkills { get; init; } = [];

    /// <summary>
    /// 获取或设置项目级技能集合。
    /// </summary>
    public IReadOnlyList<SkillMetadata> ProjectSkills { get; init; } = [];

    /// <summary>
    /// 获取技能最后刷新时间戳。
    /// </summary>
    public DateTimeOffset LastRefreshed { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 获取所有技能（项目级技能优先于用户级技能）。
    /// </summary>
    public IReadOnlyList<SkillMetadata> AllSkills
    {
        get
        {
            var projectSkillNames = ProjectSkills.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var userSkillsWithoutOverrides = UserSkills.Where(s => !projectSkillNames.Contains(s.Name));
            return [.. ProjectSkills, .. userSkillsWithoutOverrides];
        }
    }

    /// <summary>
    /// 按名称获取技能（项目级优先）。
    /// </summary>
    /// <param name="name">技能名称。</param>
    /// <returns>技能元数据，若未找到则返回 null。</returns>
    public SkillMetadata? GetSkill(string name)
    {
        return ProjectSkills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?? UserSkills.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
