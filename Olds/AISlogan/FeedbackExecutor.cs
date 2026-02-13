using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace MafDemo.Olds.AISlogan;

/// <summary>
/// 反馈评估执行器，负责评估广告语并决定流程走向
/// </summary>
internal sealed class FeedbackExecutor : Executor<SloganResult>
{
    private readonly AIAgent _agent;
    private readonly AgentSession session;

    public int MinimumRating { get; init; } = 8;    // 最低合格评分

    public int MaxAttempts { get; init; } = 3;      // 最大尝试次数

    private int _attempts;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackExecutor"/> class.
    /// </summary>
    /// <param name="id">A unique identifier for the executor.</param>
    /// <param name="chatClient">The chat client to use for the AI agent.</param>
    public FeedbackExecutor(string id, IChatClient chatClient) : base(id)
    {
        ChatClientAgentOptions agentOptions = new()
        {
            ChatOptions = new()
            {
                Instructions = "你是专业广告语编辑，你会被给一个广告语和任务，你需要根据任务提供对广告语的反馈。",
                ResponseFormat = ChatResponseFormat.ForJsonSchema<FeedbackResult>()
            }
        };

        _agent = new ChatClientAgent(chatClient, agentOptions);
        session = _agent.CreateSessionAsync().Result;
    }

    public override async ValueTask HandleAsync(SloganResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var sloganMessage = $$"""
            这是针对任务 '{{message.Task}}' 的广告语：
            广告语: {{message.Slogan}}
            请提供对这条广告语的反馈，包括评论、1到10的评分和改进建议。
            
            请按照以下JSON模式返回反馈，但切记不能"```"包裹JSON字符串，直接返回JSON字符串。

            {
                "comments": "string",
                "rating": "integer",
                "actions": "string"
            }
            
            """;

        var response = await _agent.RunAsync(sloganMessage, session, cancellationToken: cancellationToken);
        var feedback = JsonSerializer.Deserialize<FeedbackResult>(response.Text) ?? throw new InvalidOperationException("Failed to deserialize feedback.");

        await context.AddEventAsync(new FeedbackEvent(feedback), cancellationToken);

        if (feedback.Rating >= MinimumRating)
        {
            await context.YieldOutputAsync($"这条广告语被接受:\n\n{message.Slogan}", cancellationToken);
            return;
        }

        if (_attempts >= MaxAttempts)
        {
            await context.YieldOutputAsync($"这条广告语在 {MaxAttempts} 次尝试后被拒绝。最终广告语:\n\n{message.Slogan}", cancellationToken);
            return;
        }

        await context.SendMessageAsync(feedback, cancellationToken: cancellationToken);
        _attempts++;
    }
}