namespace AutoBot.SkillEngine.Execution;

/// <summary>
/// 人工确认接口，用于高风险操作的用户确认。
/// </summary>
public interface IHumanConfirmation
{
    /// <summary>
    /// 请求用户确认操作。
    /// </summary>
    /// <param name="toolName">工具名称。</param>
    /// <param name="description">操作描述。</param>
    /// <param name="riskLevel">风险等级。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>用户是否批准该操作。</returns>
    Task<bool> ConfirmAsync(
        string toolName,
        string description,
        RiskLevel riskLevel,
        CancellationToken cancellationToken = default);
}
