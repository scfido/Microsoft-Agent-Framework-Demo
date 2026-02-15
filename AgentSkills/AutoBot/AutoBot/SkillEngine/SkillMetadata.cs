namespace AutoBot.SkillEngine;

/// <summary>
/// 表示从 SKILL.md YAML frontmatter 解析的技能元数据。
/// 遵循 Agent Skills 规范：https://agentskills.io
/// </summary>
/// <param name="Name">
/// 必需。技能标识符（小写字母数字加连字符，最多 64 字符）。
/// 必须与目录名称匹配。
/// </param>
/// <param name="Description">
/// 必需。技能用途的简要描述（最多 1024 字符）。
/// </param>
/// <param name="Path">
/// 包含 SKILL.md 的技能目录的绝对路径。
/// </param>
/// <param name="License">
/// 可选。SPDX 许可证标识符（例如 "MIT"、"Apache-2.0"）。
/// </param>
/// <param name="Compatibility">
/// 可选。兼容性约束（例如 "vscode"、"cursor"、"any"）。
/// </param>
/// <param name="Metadata">
/// 可选。附加的键值元数据对。
/// </param>
/// <param name="AllowedTools">
/// 可选。技能允许使用的工具列表。
/// </param>
public sealed record SkillMetadata(
    string Name,
    string Description,
    string Path,
    string? License = null,
    string? Compatibility = null,
    IReadOnlyDictionary<string, string>? Metadata = null,
    IReadOnlyList<AllowedTool>? AllowedTools = null)
{
    /// <summary>
    /// Maximum allowed length for skill name.
    /// </summary>
    public const int MaxNameLength = 64;

    /// <summary>
    /// Maximum allowed length for skill description.
    /// </summary>
    public const int MaxDescriptionLength = 1024;

    /// <summary>
    /// Maximum file size for SKILL.md in bytes (10 MB).
    /// </summary>
    public const long MaxSkillFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// The standard skill definition filename.
    /// </summary>
    public const string SkillFileName = "SKILL.md";

    /// <summary>
    /// Gets the full path to the SKILL.md file.
    /// </summary>
    public string SkillFilePath => System.IO.Path.Combine(Path, SkillFileName);

    /// <summary>
    /// Returns a display string for the skill suitable for system prompts.
    /// </summary>
    public string ToDisplayString() => $"- **{Name}**: {Description}";
}
