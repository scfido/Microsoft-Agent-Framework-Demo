namespace AutoBot.SkillEngine;

/// <summary>
/// 表示技能验证操作的结果。
/// </summary>
public readonly struct SkillValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; }

    private SkillValidationResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static SkillValidationResult Success() => new(true, null);

    /// <summary>
    /// Creates a failed validation result with an error message.
    /// </summary>
    public static SkillValidationResult Failure(string errorMessage) => new(false, errorMessage);
}
