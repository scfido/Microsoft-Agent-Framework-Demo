using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Olds.Test01;

internal sealed class CustomEvent(string message) : WorkflowEvent(message) { }

internal sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    public override async ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.AddEventAsync(new CustomEvent($"处理反转文本: {input}"), cancellationToken);

        return new string([.. input.Reverse()]);
    }
}