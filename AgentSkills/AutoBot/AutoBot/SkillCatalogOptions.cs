namespace AutoBot;

/// <summary>
/// 技能目录与发现配置。
/// </summary>
public sealed class SkillCatalogOptions
{
    /// <summary>
    /// 获取或设置是否启用用户级技能。
    /// 默认为 true。
    /// </summary>
    public bool EnableUserSkills { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用项目级技能。
    /// 默认为 true。
    /// </summary>
    public bool EnableProjectSkills { get; set; } = true;

    /// <summary>
    /// 获取或设置用户级技能目录的自定义路径。
    /// 若为 null，使用默认路径：~/.maf/{AgentName}/skills/
    /// </summary>
    public string? UserSkillsDirectoryOverride { get; set; }

    /// <summary>
    /// 获取或设置项目级技能目录的自定义路径。
    /// 若为 null，使用默认路径：{ProjectRoot}/.maf/skills/
    /// </summary>
    public string? ProjectSkillsDirectoryOverride { get; set; }

    /// <summary>
    /// 获取或设置技能刷新间隔（秒）。
    /// 设为 0 表示不自动刷新。默认为 0。
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 0;
}
