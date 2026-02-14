using AutoBot.SkillEngine;
using System.Text.RegularExpressions;

namespace AgentSkills.Loading;

/// <summary>
/// 根据 Agent Skills 规范验证技能名称。
/// </summary>
public static partial class SkillValidator
{
    /// <summary>
    /// 有效技能名称的模式：小写字母数字加连字符，最多 64 字符。
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9][a-z0-9\-]{0,62}[a-z0-9]$|^[a-z0-9]$", RegexOptions.Compiled)]
    private static partial Regex SkillNamePattern();

    /// <summary>
    /// 根据 Agent Skills 规范验证技能名称。
    /// </summary>
    /// <param name="name">待验证的技能名称。</param>
    /// <returns>验证结果，指示成功或失败及错误消息。</returns>
    public static SkillValidationResult ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return SkillValidationResult.Failure("Skill name cannot be null or empty.");
        }

        if (name.Length > 64)
        {
            return SkillValidationResult.Failure($"Skill name exceeds maximum length of 64 characters. Actual: {name.Length}");
        }

        if (!SkillNamePattern().IsMatch(name))
        {
            return SkillValidationResult.Failure(
                "Skill name must contain only lowercase letters, numbers, and hyphens. " +
                "Must start and end with a letter or number.");
        }

        return SkillValidationResult.Success();
    }

    /// <summary>
    /// Validates that a skill name matches its directory name.
    /// </summary>
    /// <param name="skillName">The skill name from YAML frontmatter.</param>
    /// <param name="directoryName">The directory name containing the skill.</param>
    /// <returns>A validation result indicating success or failure with error message.</returns>
    public static SkillValidationResult ValidateNameMatchesDirectory(string skillName, string directoryName)
    {
        if (!string.Equals(skillName, directoryName, StringComparison.OrdinalIgnoreCase))
        {
            return SkillValidationResult.Failure(
                $"Skill name '{skillName}' does not match directory name '{directoryName}'.");
        }

        return SkillValidationResult.Success();
    }

    /// <summary>
    /// Validates a skill description.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <returns>A validation result indicating success or failure with error message.</returns>
    public static SkillValidationResult ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return SkillValidationResult.Failure("Skill description cannot be null or empty.");
        }

        if (description.Length > 1024)
        {
            return SkillValidationResult.Failure(
                $"Skill description exceeds maximum length of 1024 characters. Actual: {description.Length}");
        }

        return SkillValidationResult.Success();
    }

    /// <summary>
    /// Validates that a SKILL.md file exists and is within size limits.
    /// </summary>
    /// <param name="skillFilePath">The path to the SKILL.md file.</param>
    /// <returns>A validation result indicating success or failure with error message.</returns>
    public static SkillValidationResult ValidateSkillFile(string skillFilePath)
    {
        if (!File.Exists(skillFilePath))
        {
            return SkillValidationResult.Failure($"SKILL.md file not found at: {skillFilePath}");
        }

        var fileInfo = new FileInfo(skillFilePath);
        if (fileInfo.Length > 10 * 1024 * 1024) // 10 MB limit
        {
            return SkillValidationResult.Failure(
                $"SKILL.md file exceeds maximum size of 10 MB. Actual: {fileInfo.Length / (1024 * 1024):F2} MB");
        }

        return SkillValidationResult.Success();
    }
}
