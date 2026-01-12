using Microsoft.Agents.AI.Workflows;

namespace MafDemo.AISlogan;

/// <summary>
/// 自定义事件，用于监控工作流进度，并重写 ToString 便于控制台打印
/// </summary>
internal sealed class SloganGeneratedEvent(SloganResult sloganResult) : WorkflowEvent(sloganResult)
{
    public override string ToString() => $"广告语: {sloganResult.Slogan}";
}
