using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;

namespace MafDemo.Olds.ThreadDemo;

internal class ThreadDemoAgent
{
    private static AIAgent Build(IConfiguration configuration)
    {
        var endpoint = configuration["OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:Endpoint' 未找到");
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:ApiKey' 未找到");

        var agent = new OpenAI.Chat.ChatClient("GLM-4.5-Flash",
                     new ApiKeyCredential(apiKey),
                     new OpenAI.OpenAIClientOptions
                     {
                         Endpoint = new Uri(endpoint)
                     }
                 )
                 .AsIChatClient()
                 .AsAIAgent(new ChatClientAgentOptions
                 {
                     ChatOptions = new()
                     {
                         Instructions = "你是一个风趣幽默的聊天助手，擅长讲笑话和轻松的对话。"
                     },
                     Name = "Joker",
                     // 使用自定义的聊天消息存储，实现消息的持久化
                     //AIContextProviderFactory = (ctx, ct) => new CustomeChatMessageStore(ctx.SerializedState, context.JsonSerializerOptions)
                 });

        return agent;
    }

    public static async Task RunAsync(IConfiguration configuration)
    {
        var agent = Build(configuration);
        var session = await agent.CreateSessionAsync();
        
        await foreach (var update in agent.RunStreamingAsync("讲个笑话", session))
        {
            Console.Write(update);
        }

        Console.WriteLine();
        Console.WriteLine("-----");

        await foreach (var update in agent.RunStreamingAsync("再讲个类似的笑话。", session))
        {
            Console.Write(update);
        }

        IList<ChatMessage>? messages = session.GetService<IList<ChatMessage>>();
        Console.WriteLine($"Thread message count:{messages?.Count}");
    }
}
