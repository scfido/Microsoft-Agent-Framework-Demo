using AgentSkills.Loading;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 搜索文件工具，支持 workspace 和 skill 作用域。
/// </summary>
public sealed class SearchFilesTool
{
    private readonly RuntimeOptions _options;
    private readonly SkillsState _state;

    /// <summary>
    /// 初始化 SearchFilesTool 实例。
    /// </summary>
    public SearchFilesTool(RuntimeOptions options, SkillsState state)
    {
        _options = options;
        _state = state;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(RuntimeOptions options, SkillsState state)
    {
        var tool = new SearchFilesTool(options, state);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "search_files");
    }

    /// <summary>
    /// 搜索文件。
    /// </summary>
    [Description("按文件名模式或内容关键字搜索文件")]
    public async Task<string> ExecuteAsync(
        [Description("搜索模式（文件名或内容关键字）")] string pattern,
        [Description("作用域：workspace（默认）或 skill")] string scope = "workspace",
        [Description("当 scope=skill 时必填的技能名称")] string? skillName = null,
        [Description("搜索内容而非文件名")] bool searchContent = false,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // 占位

        try
        {
            // 解析作用域
            var toolScope = Enum.Parse<ToolScope>(scope, ignoreCase: true);
            var baseDir = ResolveBaseDirectory(toolScope, skillName);

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
                    if (fileInfo.Length > _options.Tools.MaxFileSizeBytes) continue;

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
