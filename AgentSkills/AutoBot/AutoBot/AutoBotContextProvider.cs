using AutoBot.Loading;
using AutoBot.Skills;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AutoBot;

/// <summary>
/// 本地运行时的上下文提供器，负责加载技能、生成系统提示和提供工具。
/// </summary>
public sealed class AutoBotContextProvider : AIContextProvider
{
    private readonly RuntimeOptions _options;
    private readonly SkillLoader _skillLoader;
    private SkillsState _state;

    /// <summary>
    /// 初始化新实例（从配置创建）。
    /// </summary>
    /// <param name="options">运行时配置选项。</param>
    public AutoBotContextProvider(
        RuntimeOptions options)
    {
        _options = options;
        _skillLoader = new SkillLoader();
        _state = LoadSkills();
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
        _options = restored?.Options ?? new RuntimeOptions();
        _state = restored?.State ?? new SkillsState();
        _skillLoader = new SkillLoader();
    }

    /// <summary>
    /// 在调用前提供 AI 上下文（系统提示和工具）。
    /// </summary>
    protected override ValueTask<AIContext> InvokingCoreAsync(
        InvokingContext context,
        CancellationToken cancellationToken = default)
    {
        // 生成系统提示
        var systemPrompt = SkillsSystemPromptTemplates.GenerateSystemPrompt(_state, _options);

        // 提供工具
        var factory = new ToolFactory(_options, _state, _skillLoader);

        var aiContext = new AIContext
        {
            Instructions = systemPrompt,
            Tools = factory.CreateTools().ToList()
        };

        return ValueTask.FromResult(aiContext);
    }

    /// <summary>
    /// 序列化状态以支持会话恢复。
    /// </summary>
    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var context = new RestoredContext
        {
            Options = _options,
            State = _state
        };

        return JsonSerializer.SerializeToElement(context, jsonSerializerOptions);
    }

    /// <summary>
    /// 加载技能。
    /// </summary>
    private SkillsState LoadSkills()
    {
        var userSkills = new List<SkillMetadata>();
        var projectSkills = new List<SkillMetadata>();

        // 加载用户级技能
        if (_options.SkillCatalog.EnableUserSkills)
        {
            var userDir = _options.SkillCatalog.UserSkillsDirectoryOverride
                ?? GetDefaultUserSkillsDirectory(_options.AgentName);
            userSkills.AddRange(_skillLoader.LoadSkillsFromDirectory(userDir, SkillSource.User));
        }

        // 加载项目级技能
        if (_options.SkillCatalog.EnableProjectSkills && _options.ProjectRoot != null)
        {
            var projectDir = _options.SkillCatalog.ProjectSkillsDirectoryOverride
                ?? GetDefaultProjectSkillsDirectory(_options.ProjectRoot);
            projectSkills.AddRange(_skillLoader.LoadSkillsFromDirectory(projectDir, SkillSource.Project));
        }

        return new SkillsState
        {
            UserSkills = userSkills,
            ProjectSkills = projectSkills,
            LastRefreshed = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// 获取默认用户技能目录。
    /// </summary>
    private static string GetDefaultUserSkillsDirectory(string agentName)
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".maf", agentName, "skills");
    }

    /// <summary>
    /// 获取默认项目技能目录。
    /// </summary>
    private static string GetDefaultProjectSkillsDirectory(string projectRoot)
    {
        return Path.Combine(projectRoot, ".maf", "skills");
    }

    /// <summary>
    /// 序列化/反序列化容器。
    /// </summary>
    private sealed class RestoredContext
    {
        public RuntimeOptions Options { get; set; } = new();
        public SkillsState State { get; set; } = new();
    }
}
