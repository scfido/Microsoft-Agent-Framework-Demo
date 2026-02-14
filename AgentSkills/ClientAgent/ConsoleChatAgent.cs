using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace ClientAgents;

public class ConsoleChatAgent
{
    private AIAgent agent;

    private ConsoleChatAgent(AIAgent agent)
    {
        this.agent = agent;
    }

    public static Task<ConsoleChatAgent> CreateAsync(IConfiguration configuration)
    {
        var endpoint = configuration["OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:Endpoint' 未找到");
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:ApiKey' 未找到");

        var chatClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri(endpoint)
                }
            )
            .GetChatClient("deepseek-chat")
            .AsIChatClient();

        // Create skills-enabled agent using the new factory pattern
        var agent = chatClient.AsSkillsAIAgent(
            configureSkills: options =>
            {
                options.AgentName = "my-assistant";
                options.ProjectRoot = Directory.GetCurrentDirectory();

                // Enable tools
                options.ToolsOptions.EnableReadSkillTool = true;
                options.ToolsOptions.EnableReadFileTool = true;
                options.ToolsOptions.EnableListDirectoryTool = true;
                options.ToolsOptions.EnableRunCommandTool = true;
            },
            configureAgent: options =>
            {
                options.ChatOptions = new()
                {
                    Instructions = "You are a helpful assistant with access to specialized skills."
                };
            });

        return Task.FromResult(new ConsoleChatAgent(agent));
    }


    public async Task RunningAsync()
    {
        AgentSession session = await agent.CreateSessionAsync();
        Console.WriteLine("有什么帮您呢？请输入问题：");
        while (true)
        {
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
            Console.WriteLine("---");
            Console.WriteLine("请输入：");
        }
    }
}
