using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace MafDemo.Agents.Agent_Step01_Running;

public class AgentStep01Running
{
    private AIAgent agent;

    private AgentStep01Running(AIAgent agent)
    {
        this.agent = agent;
    }

    public static AgentStep01Running Create(IConfiguration configuration)
    {
        var endpoint = configuration["OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:Endpoint' 未找到");
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:ApiKey' 未找到");

        var agent = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri(endpoint)
                }
            )
            .GetChatClient("GLM-4.7-Flash")
            .AsAIAgent(instructions: "你是一个风趣幽默的聊天助手，擅长讲笑话和轻松的对话。", name: "Joker");

        return new AgentStep01Running(agent);
    }


    public async Task Running()
    {
        await foreach (var update in agent.RunStreamingAsync("讲个笑话"))
        {
            Console.Write(update);
        }
        Console.WriteLine();
    }
}
