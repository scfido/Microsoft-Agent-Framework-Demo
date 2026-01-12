using Microsoft.Agents.AI.Workflows;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MafDemo.AISlogan;

/// <summary>
/// A custom event to indicate that feedback has been provided.
/// </summary>
internal sealed class FeedbackEvent(FeedbackResult feedbackResult) : WorkflowEvent(feedbackResult)
{
    private readonly JsonSerializerOptions _options = new() 
    { 
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  // 允许中文字符不被转义为 Unicode
    };
    public override string ToString() => $"Feedback:\n{JsonSerializer.Serialize(feedbackResult, _options)}";
}
