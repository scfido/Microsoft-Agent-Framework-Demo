using System.Text.Json.Serialization;

namespace MafDemo.Olds.AISlogan;

/// <summary>
/// 广告语结果
/// </summary>
public sealed class SloganResult
{
    [JsonPropertyName("task")]
    public required string Task { get; set; } // 任务描述

    [JsonPropertyName("slogan")]
    public required string Slogan { get; set; } // 生成的广告语
}
