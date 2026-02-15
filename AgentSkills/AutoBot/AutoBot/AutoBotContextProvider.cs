using AutoBot.SkillEngine;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AutoBot;

/// <summary>
/// 本地运行时的上下文提供器，负责加载技能、生成系统提示和提供工具。
/// </summary>
public sealed class AutoBotContextProvider : AIContextProvider
{
    private readonly AutoBotOptions options;
    private readonly SkillLoader skillLoader;
    private SkillsState skillsState;

    /// <summary>
    /// 初始化新实例（从配置创建）。
    /// </summary>
    /// <param name="options">运行时配置选项。</param>
    public AutoBotContextProvider(
        AutoBotOptions options)
    {
        this.options = options;
        skillLoader = new SkillLoader();
        skillsState = LoadSkills();
    }

    /// <summary>
    /// 初始化新实例（从序列化状态恢复）。
    /// </summary>
    /// <param name="serializedState">序列化的状态。</param>
    /// <param name="jsonSerializerOptions">JSON 序列化选项。</param>
    public AutoBotContextProvider(
        JsonElement serializedState,
        JsonSerializerOptions? jsonSerializerOptions)
    {
        // 反序列化状态和选项
        var restored = serializedState.Deserialize<RestoredContext>(jsonSerializerOptions);
        options = restored?.Options ?? new AutoBotOptions();
        skillsState = restored?.State ?? new SkillsState();
        skillLoader = new SkillLoader();
    }

    /// <summary>
    /// 在调用前提供 AI 上下文（系统提示和工具）。
    /// </summary>
    protected override ValueTask<AIContext> InvokingCoreAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        // 生成系统提示
        var systemPrompt = SkillsSystemPromptTemplates.GenerateSystemPrompt(skillsState);

        // 提供工具
        var factory = new ToolFactory(options, skillsState, skillLoader);

        var aiContext = new AIContext
        {
            Instructions = systemPrompt,
            Tools = factory.CreateTools().ToList()
        };

        return ValueTask.FromResult(aiContext);
    }
    
    /// <summary>
    /// 在调用后输出工具调用信息到控制台。
    /// </summary>
    protected override ValueTask InvokedCoreAsync(
        InvokedContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.InvokeException is not null)
        {
            return ValueTask.CompletedTask;
        }

        // 遍历响应消息，查找工具调用内容
        foreach (var message in context.ResponseMessages ?? [])
        {
            foreach (var content in message.Contents)
            {
                if (content is FunctionCallContent functionCall)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[Tool Call] {functionCall.Name}");
                    if (functionCall.Arguments is not null)
                    {
                        Console.WriteLine($"  Arguments: {JsonSerializer.Serialize(functionCall.Arguments)}");
                    }
                    Console.ResetColor();
                }
                else if (content is FunctionResultContent functionResult)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    var resultStr = functionResult.Result?.ToString() ?? "(empty)";
                    // 截断过长的结果
                    if (resultStr.Length > 500)
                    {
                        resultStr = resultStr[..500] + "... (truncated)";
                    }
                    Console.WriteLine($"[Tool Result] {functionResult.CallId}: {resultStr}");
                    Console.ResetColor();
                }
            }
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 序列化状态以支持会话恢复。
    /// </summary>
    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var context = new RestoredContext
        {
            Options = options,
            State = skillsState
        };

        return JsonSerializer.SerializeToElement(context, jsonSerializerOptions);
    }

    /// <summary>
    /// 加载技能。
    /// </summary>
    private SkillsState LoadSkills()
    {
        var skills = new List<SkillMetadata>();

        if (!string.IsNullOrEmpty(options.SkillsDirectory))
        {
            if (options.SkillNames is not null)
            {
                foreach (var name in options.SkillNames)
                {
                    var skill = skillLoader.LoadSkillByName(options.SkillsDirectory, name);
                    if (skill is not null)
                    {
                        skills.Add(skill);
                    }
                }
            }
            else
            {
                skills.AddRange(skillLoader.LoadSkillsFromDirectory(options.SkillsDirectory));
            }
        }

        return new SkillsState { Skills = skills };
    }

    /// <summary>
    /// 序列化/反序列化容器。
    /// </summary>
    private sealed class RestoredContext
    {
        public AutoBotOptions Options { get; set; } = new();
        public SkillsState State { get; set; } = new();
    }
}
