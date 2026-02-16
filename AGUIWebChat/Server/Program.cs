using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient().AddLogging();
builder.Services.AddAGUI();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();
var configuration = builder.Configuration;
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
        }
    )
    .GetChatClient(model)
    .AsIChatClient();

AIAgent agent = chatClient.AsAIAgent(
        name: "AGUIAssistant",
        instructions: "You are a helpful assistant.");

app.UseCors();

app.MapAGUI("/", agent);

await app.RunAsync();