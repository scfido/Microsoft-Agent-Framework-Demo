namespace AutoBot.SkillEngine;

/// <summary>
/// Exception thrown when skill parsing fails.
/// </summary>
public sealed class SkillParseException : Exception
{
    /// <summary>
    /// Gets the path to the skill that failed to parse.
    /// </summary>
    public string SkillPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillParseException"/> class.
    /// </summary>
    public SkillParseException(string skillPath, string message)
        : base($"Failed to parse skill at '{skillPath}': {message}")
    {
        SkillPath = skillPath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillParseException"/> class.
    /// </summary>
    public SkillParseException(string skillPath, string message, Exception innerException)
        : base($"Failed to parse skill at '{skillPath}': {message}", innerException)
    {
        SkillPath = skillPath;
    }
}
