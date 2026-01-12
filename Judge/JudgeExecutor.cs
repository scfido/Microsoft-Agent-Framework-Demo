using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Judge;


/// <summary>
/// Signals used for communication between guesses and the JudgeExecutor.
/// </summary>
internal enum NumberSignal
{
    Init,
    Above,
    Below,
}

/// <summary>
/// Executor that judges the guess and provides feedback.
/// </summary>
internal sealed class JudgeExecutor() : Executor<int>("Judge")
{
    private readonly int targetNumber;
    private int tries;

    /// <summary>
    /// Initializes a new instance of the <see cref="JudgeExecutor"/> class.
    /// </summary>
    /// <param name="targetNumber">The number to be guessed.</param>
    public JudgeExecutor(int targetNumber) : this()
    {
        this.targetNumber = targetNumber;
    }

    public override async ValueTask HandleAsync(int message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // 向下一个节点发送消息
        //await context.SendMessageAsync(NumberSignal.Below, cancellationToken: cancellationToken);
        //await context.SendMessageAsync(NumberSignal.Below, cancellationToken: cancellationToken);
        //await context.SendMessageAsync(NumberSignal.Below, cancellationToken: cancellationToken);

        tries++;
        if (message == targetNumber)
        {
            // 输出内容后工作流就结束了
            await context.YieldOutputAsync($"{targetNumber} found in {tries} tries!", cancellationToken);
        }
        else if (message < targetNumber)
        {
            await context.SendMessageAsync(NumberSignal.Below, cancellationToken: cancellationToken);
        }
        else
        {
            await context.SendMessageAsync(NumberSignal.Above, cancellationToken: cancellationToken);
        }
    }
}