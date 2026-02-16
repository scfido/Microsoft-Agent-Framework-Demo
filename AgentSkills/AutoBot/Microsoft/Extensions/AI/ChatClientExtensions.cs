using AutoBot;
using Microsoft.Agents.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Microsoft.Extensions.AI;

/// <summary>
/// 用于从 ChatClient 创建本地运行时增强的 AI Agent 的扩展方法。
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// 创建具有本地运行时支持的 AI Agent（统一技能发现 + 自动化工具）。
    /// </summary>
    /// <param name="chatClient">聊天客户端。</param>
    /// <param name="configureRuntime">可选的运行时配置回调。</param>
    /// <param name="configureAgent">可选的 Agent 配置回调。</param>
    /// <returns>具有本地运行时支持的 AI Agent。</returns>
    /// <example>
    /// <code>
    /// var agent = chatClient.AsAutoBotAgent(
    ///     configureRuntime: options =>
    ///     {
    ///         options.SkillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), ".maf", "skills");
    ///         options.SkillNames = new List&lt;string&gt; { "my-skill" };
    ///         options.EnableRunCommand = true;
    ///         options.ExecutionPolicy.AllowedCommands.Add("git");
    ///     },
    ///     configureAgent: options =>
    ///     {
    ///         options.ChatOptions = new() { Instructions = "你是一个有用的助手。" };
    ///     });
    /// </code>
    /// </example>
    public static AIAgent AsAutoBotAgent(
        this IChatClient chatClient,
        Action<AutoBotOptions>? configureRuntime = null,
        Action<ChatClientAgentOptions>? configureAgent = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        var runtimeOptions = new AutoBotOptions();
        configureRuntime?.Invoke(runtimeOptions);

        var agentOptions = new ChatClientAgentOptions
        {
            AIContextProviderFactory = (ctx, ct) =>
            {
                // 检查是否从序列化状态恢复
                if (ctx.SerializedState.ValueKind != JsonValueKind.Undefined)
                {
                    return ValueTask.FromResult<AIContextProvider>(new AutoBotContextProvider(
                        ctx.SerializedState,
                        ctx.JsonSerializerOptions));
                }

                // 创建新实例
                return ValueTask.FromResult<AIContextProvider>(new AutoBotContextProvider(
                    runtimeOptions));
            }
        };

        configureAgent?.Invoke(agentOptions);

        var agent = chatClient.AsAIAgent(agentOptions);

        // 包装 streaming middleware，将工具调用信息注入流式输出
        return agent
            .AsBuilder()
            .Use(
                runFunc: null,
                runStreamingFunc: ToolNotificationStreamingMiddleware)
            .Build();
    }

    /// <summary>
    /// 流式中间件：拦截工具调用，注入可读的工具执行提示到输出流中。
    /// </summary>
    private static async IAsyncEnumerable<AgentResponseUpdate> ToolNotificationStreamingMiddleware(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? options,
        AIAgent innerAgent,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var update in innerAgent.RunStreamingAsync(
            messages, session, options, cancellationToken))
        {
            // 检查是否包含工具调用，注入提示文本
            foreach (var content in update.Contents)
            {
                if (content is FunctionCallContent call)
                {
                    var summary = FormatToolCallSummary(call);
                    if (summary is not null)
                    {
                        yield return new AgentResponseUpdate
                        {
                            Contents = [new TextContent($"\n{summary}\n")]
                        };
                    }
                }
            }

            // 原样传递原始 update
            yield return update;
        }
    }

    /// <summary>
    /// 根据工具名称和参数生成可读的摘要文本。
    /// </summary>
    private static string? FormatToolCallSummary(FunctionCallContent call)
    {
        var args = call.Arguments;

        return call.Name switch
        {
            "list_directory" => $"📂 List directory: {GetArg(args, "relativePath") ?? "."}",
            "read_file"      => $"📄 Read file: {GetArg(args, "filePath")}",
            "write_file"     => $"✏️ Write file: {GetArg(args, "filePath")}",
            "search_files"   => $"🔍 Search: {GetArg(args, "pattern")}",
            "run_command"    => $"⚡ Run: {GetArg(args, "command")}",
            "read_skill"     => $"📖 Read skill: {GetArg(args, "skillName")}",
            _                => $"🔧 {call.Name}"
        };
    }

    private static string? GetArg(IDictionary<string, object?>? args, string key)
    {
        if (args is null) return null;
        return args.TryGetValue(key, out var value) ? value?.ToString() : null;
    }
}
