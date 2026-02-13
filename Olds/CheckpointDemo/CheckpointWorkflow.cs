using MafDemo.Olds.Test01;
using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Olds.CheckpointDemo;

internal class CheckpointWorkflow
{

    private static Workflow Build()
    {
        var step1 = new StepExecutor(1);
        var step2 = new StepExecutor(2);
        var step3 = new StepExecutor(3);

        var workflow = new WorkflowBuilder(step1)
            .AddEdge(step1, step2)
            .AddEdge(step2, step3)
            .WithOutputFrom(step3)
            .Build();

        return workflow;
    }

    public static async Task RunAsync()
    {
        // Create a checkpoint manager to manage checkpoints
        var checkpointManager = CheckpointManager.Default;
        // List to store checkpoint info for later use
        var checkpoints = new List<CheckpointInfo>();

        var workflow = Build();
        await using Checkpointed<StreamingRun> run = await InProcessExecution.StreamAsync(workflow, "Checkpoint Workflow", checkpointManager);
        await foreach (WorkflowEvent evt in run.Run.WatchStreamAsync())
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
                    break;

                case WorkflowErrorEvent error:
                    Console.WriteLine($"Workflow error: {error.Exception}");
                    return;

                case SuperStepStartedEvent superStepStarted:
                    Console.WriteLine($"Super step started: {superStepStarted.StepNumber}: {superStepStarted.Data}");
                    break;

                case SuperStepCompletedEvent superStepCompleted:
                    Console.WriteLine($"Super step completed: {superStepCompleted.StepNumber}: {superStepCompleted.Data}");
                    // Access the checkpoint and store it
                    CheckpointInfo? checkpoint = superStepCompleted.CompletionInfo!.Checkpoint;
                    if (checkpoint != null)
                    {
                        checkpoints.Add(checkpoint);
                    }
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

        Console.WriteLine();
        Console.WriteLine("-------");
        Console.WriteLine();

        await run.RestoreCheckpointAsync(checkpoints[1], CancellationToken.None);
        await foreach (WorkflowEvent evt in run.Run.WatchStreamAsync())
        {
            switch (evt)
            {
                case WorkflowOutputEvent output:
                    Console.WriteLine($"Workflow output: {output.Data}");
                    return;
            }
        }
    }
}
