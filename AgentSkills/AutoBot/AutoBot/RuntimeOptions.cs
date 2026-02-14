namespace AutoBot;

/// <summary>
/// 本地代理运行时的顶层配置选项。
/// </summary>
public sealed class RuntimeOptions
{
    // ── 基础路径 ──

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

    // ── 技能目录配置（原 SkillCatalogOptions）──

    /// <summary>
    /// 获取或设置是否启用用户级技能。默认为 true。
    /// </summary>
    public bool EnableUserSkills { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用项目级技能。默认为 true。
    /// </summary>
    public bool EnableProjectSkills { get; set; } = true;

    /// <summary>
    /// 获取或设置用户级技能目录的自定义路径。
    /// 若为 null，使用默认路径：~/.maf/{AgentName}/skills/
    /// </summary>
    public string? UserSkillsDirectoryOverride { get; set; }

    /// <summary>
    /// 获取或设置项目级技能目录的自定义路径。
    /// 若为 null，使用默认路径：{ProjectRoot}/.maf/skills/
    /// </summary>
    public string? ProjectSkillsDirectoryOverride { get; set; }

    /// <summary>
    /// 获取或设置技能刷新间隔（秒）。设为 0 表示不自动刷新。默认为 0。
    /// </summary>
    public int RefreshIntervalSeconds { get; set; } = 0;

    // ── 工具启停与资源限制（原 RuntimeToolOptions）──

    /// <summary>
    /// 获取或设置是否启用 read_file 工具。默认为 true。
    /// </summary>
    public bool EnableReadFile { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用 write_file 工具。默认为 true。
    /// </summary>
    public bool EnableWriteFile { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用 list_directory 工具。默认为 true。
    /// </summary>
    public bool EnableListDirectory { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用 search_files 工具。默认为 true。
    /// </summary>
    public bool EnableSearchFiles { get; set; } = true;

    /// <summary>
    /// 获取或设置是否启用 run_command 工具。默认为 false（需显式启用）。
    /// </summary>
    public bool EnableRunCommand { get; set; } = false;

    /// <summary>
    /// 获取或设置是否启用 read_skill 工具。默认为 true。
    /// </summary>
    public bool EnableReadSkill { get; set; } = true;

    /// <summary>
    /// 获取或设置文件操作的最大文件大小限制（字节）。默认为 10 MB。
    /// </summary>
    public int MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// 获取或设置命令执行的超时时间（秒）。默认为 60 秒。
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 获取或设置命令输出的最大大小（字节）。默认为 100 KB。
    /// </summary>
    public int MaxOutputSizeBytes { get; set; } = 100 * 1024;

    // ── 执行策略 ──

    /// <summary>
    /// 获取执行策略配置选项。
    /// </summary>
    public ExecutionPolicyOptions ExecutionPolicy { get; } = new();
}
