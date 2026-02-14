using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 写入文件工具，在 workspace 作用域内写入文件。
/// </summary>
public sealed class WriteFileTool
{
    private readonly RuntimeOptions _options;

    /// <summary>
    /// 初始化 WriteFileTool 实例。
    /// </summary>
    public WriteFileTool(RuntimeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options)
    {
        var tool = new WriteFileTool(options);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "write_file");
    }

    /// <summary>
    /// 写入文件内容。
    /// </summary>
    [Description("写入文件内容")]
    public async Task<string> ExecuteAsync(
        [Description("文件相对路径")] string filePath,
        [Description("要写入的内容")] string content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseDir = _options.WorkingDirectory;

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

}
