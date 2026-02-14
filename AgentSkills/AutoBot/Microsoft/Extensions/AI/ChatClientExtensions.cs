using AutoBot;
using Microsoft.Agents.AI;
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
    ///         options.AgentName = "my-assistant";
    ///         options.ProjectRoot = Directory.GetCurrentDirectory();
    ///         options.Tools.EnableRunCommand = true;
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
        Action<RuntimeOptions>? configureRuntime = null,
        Action<ChatClientAgentOptions>? configureAgent = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        var runtimeOptions = new RuntimeOptions();
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

        return chatClient.AsAIAgent(agentOptions);
    }
}
