using AutoBot.SkillEngine;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace AutoBot.Tools;

/// <summary>
/// 读取技能工具，返回技能的完整 SKILL.md 内容。
/// </summary>
public sealed class ReadSkillTool
{
    private readonly SkillsState _state;
    private readonly SkillLoader _skillLoader;

    /// <summary>
    /// 初始化 ReadSkillTool 实例。
    /// </summary>
    public ReadSkillTool(SkillsState state, SkillLoader skillLoader)
    {
        _state = state;
        _skillLoader = skillLoader;
    }

    /// <summary>
    /// 创建工具定义。
    /// </summary>
    public static AITool CreateTool(SkillsState state, SkillLoader skillLoader)
    {
        var tool = new ReadSkillTool(state, skillLoader);
        return AIFunctionFactory.Create(tool.ExecuteAsync, "read_skill");
    }

    /// <summary>
    /// 读取技能定义。
    /// </summary>
    [Description("读取技能的完整 SKILL.md 定义")]
    public async Task<string> ExecuteAsync(
        [Description("技能名称")] string skillName,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // 占位

        try
        {
            var skill = _state.GetSkill(skillName);
            if (skill == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = $"技能 '{skillName}' 未找到",
                    skill_name = skillName
                });
            }

            var content = _skillLoader.ReadSkillContent(skill);

            return JsonSerializer.Serialize(new
            {
                success = true,
                skill_name = skillName,
                description = skill.Description,
                source = skill.Source.ToString(),
                content
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                skill_name = skillName
            });
        }
    }
}
