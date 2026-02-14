namespace AutoBot;

/// <summary>
/// Skills系统提示模板生成器。
/// </summary>
public static class SkillsSystemPromptTemplates
{
    /// <summary>
    /// 技能系统提示词模板。
    /// </summary>
    public const string SystemPromptTemplate = """
        ## Skills System

        You have access to a skills library that provides specialized capabilities and domain knowledge.

        {skills_locations}

        **Available Skills:**

        {skills_list}

        ---

        ### How to Use Skills (Progressive Disclosure) - CRITICAL

        Skills follow a **progressive disclosure** pattern - you know they exist (name + description above),
        but you **MUST read the full instructions before using them**.

        **MANDATORY Workflow:**

        1. **Recognize when a skill applies**: Check if the user's task matches any skill's description above
        2. **Read the skill's full instructions FIRST**: Use `read_skill` tool to get the complete SKILL.md content
           - This tells you exactly what scripts exist, their parameters, and how to use them
           - **NEVER assume or guess script names, paths, or arguments**
        3. **Follow the skill's instructions precisely**: SKILL.md contains step-by-step workflows and examples
        4. **Execute scripts only after reading**: Use the exact script paths and argument formats from SKILL.md

        **IMPORTANT RULES:**

        ⚠️ **NEVER execute skill scripts without first reading the skill with `read_skill`**
        - You do NOT know what scripts exist in a skill until you read it
        - You do NOT know the correct script arguments until you read the SKILL.md
        - Guessing script names will fail - always read first

        ✅ **Correct Workflow Example:**
        ```
        User: "Split this PDF into pages"
        1. Recognize: "split-pdf" skill matches this task
        2. Call: read_skill("split-pdf") -> Get full instructions
        3. Learn: SKILL.md shows the actual script path and argument format
        4. Execute: Use the exact command format from SKILL.md
        ```

        ❌ **Wrong Workflow (DO NOT DO THIS):**
        ```
        User: "Split this PDF into pages"
        1. Recognize: "split-pdf" skill matches this task
        2. Guess command/script names before reading SKILL.md <- WRONG! Never guess!
        ```

        **Skills are Self-Documenting:**
        - Each SKILL.md tells you exactly what the skill does and how to use it
        - The skill may contain Python scripts, config files, or reference docs
        - Always use the exact paths and formats specified in SKILL.md

        Remember: **Read first, then execute.** This ensures you use skills correctly!
        """;

    /// <summary>
    /// 生成完整的系统提示（仅技能内容）。
    /// </summary>
    /// <param name="state">运行时状态。</param>
    /// <param name="options">运行时配置选项。</param>
    /// <returns>系统提示字符串。</returns>
    public static string GenerateSystemPrompt(SkillsState state, RuntimeOptions options)
    {
        var skillsList = GenerateSkillsList(state);
        var locationsDisplay = GenerateSkillsLocationsDisplay(options);

        return SystemPromptTemplate
            .Replace("{skills_locations}", locationsDisplay)
            .Replace("{skills_list}", skillsList);
    }

    /// <summary>
    /// 生成格式化的技能列表（项目级优先，用户级去重）。
    /// </summary>
    private static string GenerateSkillsList(SkillsState state)
    {
        var lines = new List<string>();

        if (state.ProjectSkills.Count > 0)
        {
            lines.Add("*Project Skills:*");
            foreach (var skill in state.ProjectSkills)
            {
                lines.Add(skill.ToDisplayString());
            }
        }

        if (state.UserSkills.Count > 0)
        {
            var projectSkillNames = state.ProjectSkills
                .Select(s => s.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var nonOverriddenUserSkills = state.UserSkills
                .Where(s => !projectSkillNames.Contains(s.Name))
                .ToList();

            if (nonOverriddenUserSkills.Count > 0)
            {
                if (lines.Count > 0)
                {
                    lines.Add(string.Empty);
                }

                lines.Add("*User Skills:*");
                foreach (var skill in nonOverriddenUserSkills)
                {
                    lines.Add(skill.ToDisplayString());
                }
            }
        }

        if (lines.Count == 0)
        {
            return "*No skills available.*";
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// 生成技能路径展示字符串。
    /// </summary>
    private static string GenerateSkillsLocationsDisplay(RuntimeOptions options)
    {
        var lines = new List<string>();

        if (options.SkillCatalog.EnableProjectSkills && !string.IsNullOrWhiteSpace(options.ProjectRoot))
        {
            var projectPath = options.SkillCatalog.ProjectSkillsDirectoryOverride
                ?? Path.Combine(options.ProjectRoot, ".maf", "skills");
            lines.Add($"- Project skills location: `{projectPath}`");
        }

        if (options.SkillCatalog.EnableUserSkills)
        {
            var userPath = options.SkillCatalog.UserSkillsDirectoryOverride
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".maf",
                    options.AgentName,
                    "skills");
            lines.Add($"- User skills location: `{userPath}`");
        }

        return lines.Count == 0
            ? "- Skills locations are disabled by runtime configuration."
            : string.Join("\n", lines);
    }
}
