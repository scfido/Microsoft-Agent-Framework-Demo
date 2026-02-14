using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 搜索文件工具，在 workspace 作用域内搜索文件。
/// </summary>
public sealed class SearchFilesTool
{
    private readonly RuntimeOptions _options;

    /// <summary>
    /// 初始化 SearchFilesTool 实例。
    /// </summary>
    public SearchFilesTool(RuntimeOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options)
    {
        var tool = new SearchFilesTool(options);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "search_files");
    }

    /// <summary>
    /// 搜索文件。
    /// </summary>
    [Description("按文件名模式或内容关键字搜索文件")]
    public async Task<string> ExecuteAsync(
        [Description("搜索模式（文件名或内容关键字）")] string pattern,
        [Description("搜索内容而非文件名")] bool searchContent = false,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // 占位

        try
        {
            var baseDir = _options.WorkingDirectory;

            var results = new List<object>();

            if (searchContent)
            {
                // 内容搜索
                SearchContentInDirectory(baseDir, pattern, baseDir, results);
            }
            else
            {
                // 文件名搜索
                var files = Directory.GetFiles(baseDir, pattern, SearchOption.AllDirectories);
                foreach (var file in files.Take(100)) // 限制结果数量
                {
                    var relativePath = Path.GetRelativePath(baseDir, file);
                    var size = new FileInfo(file).Length;
                    results.Add(new { path = relativePath, size_bytes = size });
                }
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                pattern,
                search_type = searchContent ? "content" : "filename",
                results_count = results.Count,
                results
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                pattern
            });
        }
    }

    private void SearchContentInDirectory(string directory, string pattern, string baseDir, List<object> results)
    {
        if (results.Count >= 100) return; // 限制结果数量

        try
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length > _options.MaxFileSizeBytes) continue;

                    var content = File.ReadAllText(file);
                    if (content.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        var relativePath = Path.GetRelativePath(baseDir, file);
                        results.Add(new { path = relativePath, size_bytes = fileInfo.Length });
                    }
                }
                catch
                {
                    // 忽略无法读取的文件
                }

                if (results.Count >= 100) return;
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                SearchContentInDirectory(subDir, pattern, baseDir, results);
                if (results.Count >= 100) return;
            }
        }
        catch
        {
            // 忽略无法访问的目录
        }
    }

}
