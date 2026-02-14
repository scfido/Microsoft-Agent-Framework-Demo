using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 列出目录工具，支持 workspace 和 skill 作用域。
/// </summary>
public sealed class ListDirectoryTool
{
    private readonly RuntimeOptions _options;
    private readonly SkillsState _state;

    /// <summary>
    /// 初始化 ListDirectoryTool 实例。
    /// </summary>
    public ListDirectoryTool(RuntimeOptions options, SkillsState state)
    {
        _options = options;
        _state = state;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options, SkillsState state)
    {
        var tool = new ListDirectoryTool(options, state);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "list_directory");
    }

    /// <summary>
    /// 列出目录内容。
    /// </summary>
    [Description("列出目录内容")]
    public async Task<string> ExecuteAsync(
        [Description("目录相对路径（可选，默认为根目录）")] string? relativePath = null,
        [Description("作用域：workspace（默认）或 skill")] string scope = "workspace",
        [Description("当 scope=skill 时必填的技能名称")] string? skillName = null,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // 占位，保持异步签名

        try
        {
            // 解析作用域
            var toolScope = Enum.Parse<ToolScope>(scope, ignoreCase: true);
            var baseDir = ResolveBaseDirectory(toolScope, skillName);

            // 安全解析路径
            var targetDir = string.IsNullOrEmpty(relativePath)
                ? baseDir
                : PathSecurity.ResolveSafePath(baseDir, relativePath);

            if (targetDir == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "路径遍历尝试被拒绝",
                    path = relativePath
                });
            }

            if (!Directory.Exists(targetDir))
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "目录不存在",
                    path = relativePath
                });
            }

            // 列出文件和目录
            var entries = new List<object>();

            foreach (var dir in Directory.GetDirectories(targetDir))
            {
                var name = Path.GetFileName(dir);
                entries.Add(new { name, is_directory = true });
            }

            foreach (var file in Directory.GetFiles(targetDir))
            {
                var name = Path.GetFileName(file);
                var size = new FileInfo(file).Length;
                entries.Add(new { name, is_directory = false, size_bytes = size });
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                path = relativePath ?? ".",
                entries
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                path = relativePath
            });
        }
    }

    private string ResolveBaseDirectory(ToolScope scope, string? skillName)
    {
        return scope switch
        {
            ToolScope.Workspace => _options.WorkingDirectory,
            ToolScope.Skill => ResolveSkillDirectory(skillName ?? throw new ArgumentException("skillName 在 scope=skill 时必填")),
            _ => throw new ArgumentException($"不支持的作用域: {scope}")
        };
    }

    private string ResolveSkillDirectory(string skillName)
    {
        var skill = _state.GetSkill(skillName);
        if (skill == null)
        {
            throw new ArgumentException($"技能 '{skillName}' 未找到");
        }
        return skill.Path;
    }
}
