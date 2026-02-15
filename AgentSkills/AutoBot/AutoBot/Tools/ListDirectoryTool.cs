using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 列出目录工具，在 workspace 作用域内列出目录内容。
/// </summary>
public sealed class ListDirectoryTool
{
    private readonly AutoBotOptions _options;

    /// <summary>
    /// 初始化 ListDirectoryTool 实例。
    /// </summary>
    public ListDirectoryTool(AutoBotOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(AutoBotOptions options)
    {
        var tool = new ListDirectoryTool(options);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "list_directory");
    }

    /// <summary>
    /// 列出目录内容。
    /// </summary>
    [Description("列出目录内容")]
    public async Task<string> ExecuteAsync(
        [Description("目录相对路径（可选，默认为根目录）")] string? relativePath = null,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // 占位，保持异步签名

        try
        {
            var baseDir = _options.WorkingDirectory;

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

}
