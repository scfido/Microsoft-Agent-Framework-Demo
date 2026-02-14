using System.Text.RegularExpressions;

namespace AutoBot.SkillEngine;

/// <summary>
/// 表示技能的 allowed-tools 字段中的允许工具规范。
/// 支持精确匹配和 glob 模式。
/// </summary>
/// <param name="Name">工具名称或模式（例如 "read_file" 或 "execute_*"）。</param>
/// <param name="IsPattern">是否表示 glob 模式。</param>
public sealed record AllowedTool(string Name, bool IsPattern)
{
    private Regex? _regex;

    /// <summary>
    /// 检查给定的工具名称是否匹配此允许的工具规范。
    /// </summary>
    /// <param name="toolName">要检查的工具名称。</param>
    /// <returns>如果工具名称匹配则返回 true，否则返回 false。</returns>
    public bool Matches(string toolName)
    {
        if (!IsPattern)
        {
            return string.Equals(Name, toolName, StringComparison.OrdinalIgnoreCase);
        }

        _regex ??= new Regex(
            "^" + Regex.Escape(Name).Replace("\\*", ".*") + "$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        return _regex.IsMatch(toolName);
    }

    /// <summary>
    /// 将以空格分隔的 allowed-tools 字符串解析为单独的 <see cref="AllowedTool"/> 实例。
    /// </summary>
    /// <param name="allowedToolsString">以空格分隔的工具名称/模式字符串。</param>
    /// <returns>已解析的允许工具集合。</returns>
    public static IReadOnlyList<AllowedTool> Parse(string? allowedToolsString)
    {
        if (string.IsNullOrWhiteSpace(allowedToolsString))
        {
            return [];
        }

        var tools = new List<AllowedTool>();
        var parts = allowedToolsString.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            var isPattern = part.Contains('*');
            tools.Add(new AllowedTool(part, isPattern));
        }

        return tools;
    }
}
