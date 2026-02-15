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
    /// <returns>系统提示字符串。</returns>
    public static string GenerateSystemPrompt(SkillsState state)
    {
        var skillsList = GenerateSkillsList(state);

        return SystemPromptTemplate
            .Replace("{skills_list}", skillsList);
    }

    /// <summary>
    /// 生成格式化的技能列表。
    /// </summary>
    private static string GenerateSkillsList(SkillsState state)
    {
        if (state.Skills.Count == 0)
            return "*No skills available.*";

        var lines = state.Skills.Select(s => s.ToDisplayString());
        return string.Join("\n", lines);
    }
}
