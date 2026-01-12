using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.Test01;

internal class Test01Workflow
{

    private static Workflow Build()
    {
        var uppercase = new UppercaseExecutor();
        var reverse = new ReverseTextExecutor();

        var inputPort = RequestPort.Create<string, string>("input-port");

        var workflow = new WorkflowBuilder(uppercase)
            .AddEdge(uppercase, inputPort)
            .AddEdge(inputPort, reverse)
            .WithOutputFrom(reverse)
            .Build();

        return workflow;
    }

    public static async Task RunAsync()
    {
        var workflow = Build();
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, "Hello, World!");
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                // 每个Executor的启动事件
                case ExecutorInvokedEvent invoke:
                    Console.WriteLine($"Executor started: {invoke.ExecutorId}");
                    break;

                case ExecutorCompletedEvent complete:
                    Console.WriteLine($"Executor completed: {complete.ExecutorId}: {complete.Data}");
                    break;

                // 整个工作流的完成输出事件
                // 如果Build中没有WithOutputFrom，则不会触发这个事件
                case WorkflowOutputEvent output:
                    Console.WriteLine($"Workflow output: {output.Data}");
                    return;

                case WorkflowErrorEvent error:
                    Console.WriteLine($"Workflow error: {error.Exception}");
                    return;

                case SuperStepStartedEvent superStepStarted:
                    Console.WriteLine($"Super step started: {superStepStarted.StepNumber}: {superStepStarted.Data}");
                    break;

                case SuperStepCompletedEvent superStepCompleted:
                    Console.WriteLine($"Super step completed: {superStepCompleted.StepNumber}: {superStepCompleted.Data}");
                    break;

                // 将工作流中断，等待用户处理的请求事件
                case RequestInfoEvent requestInfo:
                    Console.WriteLine("请输入内容：");
                    string? input = Console.ReadLine() ?? "用户没有输入内容";
                    ExternalResponse response = requestInfo.Request.CreateResponse<string>(input);
                    await run.SendResponseAsync(response).ConfigureAwait(false);
                    break;

                // 从Executor中发出的自定义事件
                case CustomEvent customEvent:
                    Console.WriteLine($"Custom event: {customEvent.Data}");
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Unknown event: ");
                    Console.ResetColor();
                    Console.WriteLine(evt.Data?.ToString());
                    break;
            }
        }
    }
}
