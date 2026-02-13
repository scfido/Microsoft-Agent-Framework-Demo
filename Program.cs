using MafDemo.Agents.Agent_Step01_Running;
using MafDemo.Agents.Agent_Step02_MultiturnConversation;
using MafDemo.Olds.ThreadDemo;
using Microsoft.Extensions.Configuration;

namespace MafDemo;

internal class Program
{
    static async Task Main(string[] args)
    {
        // 构建配置
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        //await TestWorkflow.RunAsync();
        //await JudgeWorkflow.RunAsync();
        //await SloganWorkflow.RunAsync(configuration);
        //await CheckpointWorkflow.RunAsync();
        //await ThreadDemoAgent.RunAsync(configuration);

        //await AgentStep01Running.Create(configuration)
        //    .Running();

        await (await AgentStep02MultiturnConversation.CreateAsync(configuration))
            .RunningAsync();
    }
}
