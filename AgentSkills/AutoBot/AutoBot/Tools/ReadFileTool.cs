using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 读取文件工具，在 workspace 作用域内读取文件。
/// </summary>
public sealed class ReadFileTool
{
    private readonly RuntimeOptions _options;

    /// <summary>
    /// 初始化 ReadFileTool 实例。
    /// </summary>
    public ReadFileTool(RuntimeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options)
    {
        var tool = new ReadFileTool(options);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "read_file");
    }

    /// <summary>
    /// 读取文件内容。
    /// </summary>
    [Description("读取文件内容")]
    public async Task<string> ExecuteAsync(
        [Description("文件相对路径")] string filePath,
        [Description("起始行号（从 1 开始）")] int? offset = null,
        [Description("读取行数限制")] int? limit = null,
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

            if (!File.Exists(safePath))
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "文件不存在",
                    file_path = filePath
                });
            }

            // 检查文件大小
            var fileInfo = new FileInfo(safePath);
            if (fileInfo.Length > _options.MaxFileSizeBytes)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"文件大小 ({fileInfo.Length} 字节) 超过限制 ({_options.MaxFileSizeBytes} 字节)",
                    file_path = filePath
                });
            }

            // 读取文件内容
            var content = await File.ReadAllTextAsync(safePath, cancellationToken);

            // 应用偏移和限制
            if (offset.HasValue || limit.HasValue)
            {
                var lines = content.Split('\n');
                var startLine = Math.Max(0, (offset ?? 1) - 1);
                var count = limit ?? (lines.Length - startLine);
                content = string.Join('\n', lines.Skip(startLine).Take(count));
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                file_path = filePath,
                size_bytes = fileInfo.Length,
                content
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
