using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Test01;

internal sealed class StepExecutor : Executor<string, string>
{
    private int step;
    private string? state = string.Empty;

    public StepExecutor(int step)
        : base($"StepExecutor{step}")
    {
        this.step = step;
    }

    public override async ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Executing StepExecutor{step} with input: {input} and state: {state}");
        state = input + $" -> Step{step}";
        await context.YieldOutputAsync(state, cancellationToken);
        return state;
    }


    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellation = default)
    {
        Console.WriteLine($"Checkpointing Step{step} with state: {state}");
        return context.QueueStateUpdateAsync($"StepExecutorState-{step}", state, cancellationToken: cancellation);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context, CancellationToken cancellation = default)
    {
        state = await context.ReadStateAsync<string>($"StepExecutorState-{step}", cancellationToken: cancellation).ConfigureAwait(false);
        Console.WriteLine($"Restore Step{step} with state: {state}");
    }
}
