using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoBot.SkillEngine;

/// <summary>
/// 从配置的目录中发现、验证和加载技能。
/// </summary>
public sealed class SkillLoader
{
    private readonly SkillParser parser;
    private readonly ILogger<SkillLoader> logger;

    /// <summary>
    /// 初始化 <see cref="SkillLoader"/> 的新实例。
    /// </summary>
    /// <param name="logger">可选的日志记录器实例。</param>
    public SkillLoader(ILogger<SkillLoader>? logger = null)
    {
        parser = new SkillParser();
        this.logger = logger ?? NullLogger<SkillLoader>.Instance;
    }

    /// <summary>
    /// 从指定目录加载所有技能。
    /// </summary>
    /// <param name="skillsDirectory">包含技能子目录的目录。</param>
    /// <returns>成功加载的技能集合。</returns>
    public IEnumerable<SkillMetadata> LoadSkillsFromDirectory(string skillsDirectory)
    {
        if (!Directory.Exists(skillsDirectory))
        {
            logger.LogDebug("Skills directory does not exist: {Directory}", skillsDirectory);
            yield break;
        }

        var skillDirectories = Directory.GetDirectories(skillsDirectory);

        foreach (var skillDir in skillDirectories)
        {
            var skill = TryLoadSkill(skillDir);
            if (skill is not null)
            {
                yield return skill;
            }
        }
    }

    /// <summary>
    /// 按名称从指定目录加载单个技能。
    /// </summary>
    /// <param name="skillsDirectory">包含技能子目录的根目录。</param>
    /// <param name="skillName">要加载的技能名称（对应子目录名）。</param>
    /// <returns>加载的技能元数据，如果加载失败则返回 null。</returns>
    public SkillMetadata? LoadSkillByName(string skillsDirectory, string skillName)
    {
        var skillDir = Path.Combine(skillsDirectory, skillName);
        return TryLoadSkill(skillDir);
    }

    /// <summary>
    /// 尝试从目录加载技能。
    /// </summary>
    /// <param name="skillDirectory">技能目录路径。</param>
    /// <returns>加载的技能元数据，如果加载失败则返回 null。</returns>
    private SkillMetadata? TryLoadSkill(string skillDirectory)
    {
        var skillFilePath = Path.Combine(skillDirectory, SkillMetadata.SkillFileName);

        if (!File.Exists(skillFilePath))
        {
            logger.LogDebug("No SKILL.md found in: {Directory}", skillDirectory);
            return null;
        }

        // Security check: ensure the skill file is not a symlink pointing outside
        if (PathSecurity.IsSymbolicLink(skillFilePath))
        {
            var realPath = PathSecurity.GetRealPath(skillFilePath);
            if (realPath is null || !PathSecurity.IsPathSafe(realPath, skillDirectory))
            {
                logger.LogWarning(
                    "Skipping skill with potentially unsafe symlink: {Directory}",
                    skillDirectory);
                return null;
            }
        }

        try
        {
            var skill = parser.Parse(skillFilePath);
            logger.LogDebug("Loaded skill: {SkillName}", skill.Name);
            return skill;
        }
        catch (SkillParseException ex)
        {
            logger.LogWarning("Failed to parse skill: {Error}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error loading skill from: {Directory}", skillDirectory);
            return null;
        }
    }

    /// <summary>
    /// 读取 SKILL.md 文件的完整内容。
    /// </summary>
    /// <param name="skill">技能元数据。</param>
    /// <returns>SKILL.md 文件的完整内容。</returns>
    /// <exception cref="FileNotFoundException">如果文件不存在则抛出。</exception>
    public string ReadSkillContent(SkillMetadata skill)
    {
        var skillFilePath = skill.SkillFilePath;

        if (!File.Exists(skillFilePath))
        {
            throw new FileNotFoundException($"SKILL.md not found for skill '{skill.Name}'", skillFilePath);
        }

        return File.ReadAllText(skillFilePath);
    }

    /// <summary>
    /// 列出技能目录中的文件。
    /// </summary>
    /// <param name="skill">技能元数据。</param>
    /// <param name="relativePath">技能目录内的可选相对路径。</param>
    /// <returns>文件和目录条目的集合。</returns>
    public IEnumerable<SkillDirectoryEntry> ListSkillDirectory(SkillMetadata skill, string? relativePath = null)
    {
        var targetDir = skill.Path;

        if (!string.IsNullOrEmpty(relativePath))
        {
            var safePath = PathSecurity.ResolveSafePath(skill.Path, relativePath);
            if (safePath is null)
            {
                throw new UnauthorizedAccessException($"Path traversal attempt detected: {relativePath}");
            }
            targetDir = safePath;
        }

        if (!Directory.Exists(targetDir))
        {
            yield break;
        }

        foreach (var dir in Directory.GetDirectories(targetDir))
        {
            var name = Path.GetFileName(dir);
            yield return new SkillDirectoryEntry(name, true);
        }

        foreach (var file in Directory.GetFiles(targetDir))
        {
            var name = Path.GetFileName(file);
            var size = new FileInfo(file).Length;
            yield return new SkillDirectoryEntry(name, false, size);
        }
    }
}

/// <summary>
/// 表示技能目录列表中的条目。
/// </summary>
/// <param name="Name">文件或目录的名称。</param>
/// <param name="IsDirectory">是否为目录。</param>
/// <param name="Size">文件大小（字节），仅用于文件。</param>
public sealed record SkillDirectoryEntry(string Name, bool IsDirectory, long? Size = null);
