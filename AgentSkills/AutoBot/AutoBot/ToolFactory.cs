using AutoBot.SkillEngine;
using AutoBot.Tools;
using Microsoft.Extensions.AI;

namespace AutoBot;

/// <summary>
/// 本地运行时工具工厂，统一创建所有运行时工具。
/// </summary>
public sealed class ToolFactory
{
    private readonly AutoBotOptions options;
    private readonly SkillsState skillsState;
    private readonly SkillLoader skillLoader;

    /// <summary>
    /// 初始化工具工厂。
    /// </summary>
    /// <param name="options">运行时配置选项。</param>
    /// <param name="state">运行时状态。</param>
    /// <param name="skillLoader">技能加载器。</param>
    public ToolFactory(
        AutoBotOptions options,
        SkillsState state,
        SkillLoader skillLoader)
    {
        this.options = options;
        skillsState = state;
        this.skillLoader = skillLoader;
    }

    /// <summary>
    /// 创建所有启用的工具。
    /// </summary>
    /// <returns>工具集合。</returns>
    public IReadOnlyList<AITool> CreateTools()
    {
        var tools = new List<AITool>();

        if (options.EnableReadFile)
        {
            tools.Add(ReadFileTool.CreateTool(options));
        }

        if (options.EnableWriteFile)
        {
            tools.Add(WriteFileTool.CreateTool(options));
        }

        if (options.EnableListDirectory)
        {
            tools.Add(ListDirectoryTool.CreateTool(options));
        }

        if (options.EnableSearchFiles)
        {
            tools.Add(SearchFilesTool.CreateTool(options));
        }

        if (options.EnableRunCommand)
        {
            tools.Add(RunCommandTool.CreateTool(options));
        }

        if (options.EnableReadSkill && skillsState.AllSkills.Count > 0)
        {
            tools.Add(ReadSkillTool.CreateTool(skillsState, skillLoader));
        }

        return tools;
    }
}
