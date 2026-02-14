using AutoBot.SkillEngine;
using AutoBot.Tools;
using Microsoft.Extensions.AI;

namespace AutoBot;

/// <summary>
/// 本地运行时工具工厂，统一创建所有运行时工具。
/// </summary>
public sealed class ToolFactory
{
    private readonly RuntimeOptions _options;
    private readonly SkillsState _state;
    private readonly SkillLoader _skillLoader;

    /// <summary>
    /// 初始化工具工厂。
    /// </summary>
    /// <param name="options">运行时配置选项。</param>
    /// <param name="state">运行时状态。</param>
    /// <param name="skillLoader">技能加载器。</param>
    public ToolFactory(
        RuntimeOptions options,
        SkillsState state,
        SkillLoader skillLoader)
    {
        _options = options;
        _state = state;
        _skillLoader = skillLoader;
    }

    /// <summary>
    /// 创建所有启用的工具。
    /// </summary>
    /// <returns>工具集合。</returns>
    public IReadOnlyList<AITool> CreateTools()
    {
        var tools = new List<AITool>();

        if (_options.EnableReadFile)
        {
            tools.Add(ReadFileTool.CreateTool(_options));
        }

        if (_options.EnableWriteFile)
        {
            tools.Add(WriteFileTool.CreateTool(_options));
        }

        if (_options.EnableListDirectory)
        {
            tools.Add(ListDirectoryTool.CreateTool(_options));
        }

        if (_options.EnableSearchFiles)
        {
            tools.Add(SearchFilesTool.CreateTool(_options));
        }

        if (_options.EnableRunCommand)
        {
            tools.Add(RunCommandTool.CreateTool(_options));
        }

        if (_options.EnableReadSkill && _state.AllSkills.Count > 0)
        {
            tools.Add(ReadSkillTool.CreateTool(_state, _skillLoader));
        }

        return tools;
    }
}
