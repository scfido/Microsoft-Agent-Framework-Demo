using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });

        var endpoint = configuration["OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:Endpoint' 未找到");
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:ApiKey' 未找到");
        var model = configuration["OpenAI:Model"] ?? "gpt-4o";

        var chatClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions()
                {
                    Endpoint = new Uri(endpoint),
                    //ClientLoggingOptions = new System.ClientModel.Primitives.ClientLoggingOptions()
                    //{
                    //    EnableLogging = true, // 开启日志
                    //    LoggerFactory = loggerFactory, // 使用上面创建的 LoggerFactory
                    //    EnableMessageContentLogging = true, // 打印消息内容
                    //}
                }
            )
            .GetChatClient(model)
            .AsIChatClient();

        var agent = chatClient.AsAutoBotAgent(options =>
            {
                options.AgentName = "my-assistant";
                options.ProjectRoot = Directory.GetCurrentDirectory();

                // Enable Skill tools
                options.Tools.EnableReadSkill = true;
                options.Tools.EnableReadFile = true;
                options.Tools.EnableListDirectory = true;
                options.Tools.EnableRunCommand = true;
                options.Tools.EnableSearchFiles = true;
                options.Tools.EnableWriteFile = true;
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
