using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Olds.Test01;

internal sealed class UppercaseExecutor() : Executor<string, string>("UppercaseExecutor")
{
    public override async ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        await context.YieldOutputAsync("测试在Executor中输出内容", cancellationToken);
        return input.ToUpper();
    }
}
