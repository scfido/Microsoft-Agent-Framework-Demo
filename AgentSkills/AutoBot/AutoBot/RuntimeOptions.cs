namespace AutoBot;

/// <summary>
/// 本地代理运行时的顶层配置选项。
/// 聚合技能目录、工具设置和执行策略。
/// </summary>
public sealed class RuntimeOptions
{
    /// <summary>
    /// 获取或设置代理名称，用于确定用户级技能目录。
    /// </summary>
    public string AgentName { get; set; } = "default-agent";

    /// <summary>
    /// 获取或设置项目根目录，用于确定项目级技能目录。
    /// </summary>
    public string? ProjectRoot { get; set; }

    /// <summary>
    /// 获取或设置工作目录（workspace 沙箱根目录）。
    /// 默认为当前目录。
    /// </summary>
    public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// 获取技能目录配置选项。
    /// </summary>
    public SkillCatalogOptions SkillCatalog { get; } = new();

    /// <summary>
    /// 获取运行时工具配置选项。
    /// </summary>
    public RuntimeToolOptions Tools { get; } = new();

    /// <summary>
    /// 获取执行策略配置选项。
    /// </summary>
    public ExecutionPolicyOptions ExecutionPolicy { get; } = new();
}
