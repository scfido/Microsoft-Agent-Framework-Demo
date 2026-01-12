using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using System.Text.Json;

namespace MafDemo.AISlogan;

/// <summary>
/// 广告语生成 / 优化执行器
/// A custom executor that uses an AI agent to generate slogans based on a given task.
/// Note that this executor has two message handlers:
/// 1. HandleAsync(string message): Handles the initial task to create a slogan.
/// 2. HandleAsync(Feedback message): Handles feedback to improve the slogan.
/// </summary>
internal sealed class SloganWriterExecutor : Executor
{
    private readonly AIAgent _agent;
    private readonly AgentThread _thread;

    /// <summary>
    /// Initializes a new instance of the <see cref="SloganWriterExecutor"/> class.
    /// </summary>
    /// <param name="id">A unique identifier for the executor.</param>
    /// <param name="chatClient">The chat client to use for the AI agent.</param>
    public SloganWriterExecutor(string id, IChatClient chatClient) : base(id)
    {
        ChatClientAgentOptions agentOptions = new()
        {
            ChatOptions = new()
            {
                Instructions = $$"""
                    你是个专业广告语写手，你会被给一个任务，你需要根据任务生成一个广告语。

                    请按照以下JSON模式返回广告语，但切记不能"```"包裹JSON字符串，直接返回JSON字符串。

                    {
                        "task": "string",
                        "slogan": "string"
                    } 

                 """,
                
                ResponseFormat = ChatResponseFormat.ForJsonSchema<SloganResult>(),
            }
        };

        _agent = new ChatClientAgent(chatClient, agentOptions);
        _thread = _agent.GetNewThread();
    }

    protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<string, SloganResult>(HandleAsync)  // 处理初始字符串任务
                    .AddHandler<FeedbackResult, SloganResult>(HandleAsync); // 处理反馈优化

    /// <summary>
    /// 处理初始任务，生成第一版广告语
    /// </summary>
    public async ValueTask<SloganResult> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var result = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);

         var sloganResult = JsonSerializer.Deserialize<SloganResult>(result.Text) ?? throw new InvalidOperationException("Failed to deserialize slogan result.");
        //var sloganResult = new SloganResult { Task = message, Slogan = result.Text };
        await context.AddEventAsync(new SloganGeneratedEvent(sloganResult), cancellationToken);
        return sloganResult;
    }

    /// <summary>
    /// 处理反馈，优化广告语
    /// </summary>
    public async ValueTask<SloganResult> HandleAsync(FeedbackResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var feedbackMessage = $"""
            这是你对上一次广告语的反馈：
            评论: {message.Comments}
            评分: {message.Rating}
            建议: {message.Actions}

            请使用这些反馈来改进你的广告语。
            """;

        var result = await _agent.RunAsync(feedbackMessage, _thread, cancellationToken: cancellationToken);
        // var sloganResult = JsonSerializer.Deserialize<SloganResult>(result.Text) ?? throw new InvalidOperationException("Failed to deserialize slogan result.");
        var sloganResult = new SloganResult { Task = message.Actions, Slogan = result.Text };

        await context.AddEventAsync(new SloganGeneratedEvent(sloganResult), cancellationToken);
        return sloganResult;
    }
}
