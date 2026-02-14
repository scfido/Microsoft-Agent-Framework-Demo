using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 写入文件工具，支持 workspace 和 skill 作用域。
/// </summary>
public sealed class WriteFileTool
{
    private readonly RuntimeOptions _options;
    private readonly SkillsState _state;

    /// <summary>
    /// 初始化 WriteFileTool 实例。
    /// </summary>
    public WriteFileTool(RuntimeOptions options, SkillsState state)
    {
        _options = options;
        _state = state;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options, SkillsState state)
    {
        var tool = new WriteFileTool(options, state);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "write_file");
    }

    /// <summary>
    /// 写入文件内容。
    /// </summary>
    [Description("写入文件内容")]
    public async Task<string> ExecuteAsync(
        [Description("文件相对路径")] string filePath,
        [Description("要写入的内容")] string content,
        [Description("作用域：workspace（默认）或 skill")] string scope = "workspace",
        [Description("当 scope=skill 时必填的技能名称")] string? skillName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 解析作用域
            var toolScope = Enum.Parse<ToolScope>(scope, ignoreCase: true);
            var baseDir = ResolveBaseDirectory(toolScope, skillName);

            // 安全解析路径
            var safePath = PathSecurity.ResolveSafePath(baseDir, filePath);
            if (safePath == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "路径遍历尝试被拒绝",
                    file_path = filePath
                });
            }

            // 确保目录存在
            var directory = Path.GetDirectoryName(safePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 写入文件
            await File.WriteAllTextAsync(safePath, content, cancellationToken);

            var fileInfo = new FileInfo(safePath);
            return JsonSerializer.Serialize(new
            {
                success = true,
                file_path = filePath,
                size_bytes = fileInfo.Length,
                message = "文件写入成功"
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                file_path = filePath
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
