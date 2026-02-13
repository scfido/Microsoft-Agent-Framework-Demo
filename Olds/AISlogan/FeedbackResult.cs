using System.Text.Json.Serialization;

namespace MafDemo.Olds.AISlogan;

/// <summary>
/// 反馈结果
/// </summary>
public sealed class FeedbackResult
{
    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("actions")]
    public string Actions { get; set; } = string.Empty;
}
