using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace GettingStarted.Agents.Agent_Step02_MultiturnConversation;

public class AgentStep02MultiturnConversation
{
    private AIAgent agent;

    private AgentStep02MultiturnConversation(AIAgent agent)
    {
        this.agent = agent;
    }

    public static async Task<AgentStep02MultiturnConversation> CreateAsync(IConfiguration configuration)
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

        return new AgentStep02MultiturnConversation(agent);
    }


    public async Task RunningAsync()
    {
        AgentSession session = await agent.CreateSessionAsync();
        while (true)
        {
            Console.Write("请输入：");
            string input = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                break;
            }
            
            await foreach (var update in agent.RunStreamingAsync(input, session))
            {
                Console.Write(update);
            }
            Console.WriteLine();
        }
    }
}
