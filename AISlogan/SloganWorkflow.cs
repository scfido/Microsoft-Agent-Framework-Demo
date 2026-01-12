using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using Microsoft.Extensions.Logging;

namespace MafDemo.AISlogan;

internal class SloganWorkflow
{
    private static IChatClient? chatClient;

    private static void Initialize(IConfiguration configuration)
    {
        var loggerFactory = LoggerFactory.Create(b =>
        {
            b.AddConsole();
            // b.SetMinimumLevel(LogLevel.Trace);
        });

        var endpoint = configuration["OpenAI:Endpoint"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:Endpoint' 未找到");
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("配置项 'OpenAI:ApiKey' 未找到");
            
        chatClient = new LoggingChatClient(
                new OpenAI.Chat.ChatClient("GLM-4.5-Flash",
                    new ApiKeyCredential(apiKey),
                    new OpenAI.OpenAIClientOptions
                    {
                        Endpoint = new Uri(endpoint)
                    }
                ).AsIChatClient(),
                loggerFactory.CreateLogger<LoggingChatClient>()
        );
    }
    private static Workflow Build()
    {
        if (chatClient == null)
        {
            throw new InvalidOperationException("ChatClient 未初始化。请先调用 Initialize 方法。");
        }

        // Create the executors
        var sloganWriter = new SloganWriterExecutor("SloganWriter", chatClient);
        var feedbackProvider = new FeedbackExecutor("FeedbackProvider", chatClient);

        // Build the workflow by adding executors and connecting them
        var workflow = new WorkflowBuilder(sloganWriter)
            .AddEdge(sloganWriter, feedbackProvider)
            .AddEdge(feedbackProvider, sloganWriter)
            .WithOutputFrom(feedbackProvider)
            .Build();

        return workflow;
    }

    public static async Task RunAsync(IConfiguration configuration)
    {
        if (chatClient == null)
        {
            Initialize(configuration);
        }
        var workflow = Build();
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, input: "为一款新的电动SUV创建一个广告语，这款SUV既实惠又好玩驾驶。");
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is SloganGeneratedEvent or FeedbackEvent)
            {
                // Custom events to allow us to monitor the progress of the workflow.
                Console.WriteLine($"{evt}");
            }

            if (evt is WorkflowOutputEvent outputEvent)
            {
                Console.WriteLine($"{outputEvent}");
            }

            if (evt is WorkflowErrorEvent errorEvent)
            {
                Console.WriteLine($"Workflow error: {errorEvent.Exception?.Message ?? "未知错误"}");
            }
        }
    }
}
