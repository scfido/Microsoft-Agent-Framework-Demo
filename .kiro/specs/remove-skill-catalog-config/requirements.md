# 需求文档：移除技能目录配置

## 简介

当前 AutoBot 运行时通过 `RuntimeOptions` 中的多个技能目录配置属性（`AgentName`、`ProjectRoot`、`EnableUserSkills`、`EnableProjectSkills`、`UserSkillsDirectoryOverride`、`ProjectSkillsDirectoryOverride`、`RefreshIntervalSeconds`）在运行时自动扫描多个目录来发现技能。此架构过于复杂，将运行时与 User/Project 目录结构紧密耦合。

本次重构的目标是简化为：应用层在 `RuntimeOptions` 中指定一个技能目录路径和一组技能名称，运行时从该目录中按名称加载指定技能。

## 术语表

- **RuntimeOptions**: 运行时顶层配置类，包含工具开关、资源限制、执行策略等运行时行为配置。
- **SkillsState**: 运行时状态容器，持有已加载的技能集合。
- **SkillMetadata**: 技能元数据记录，包含名称、描述、路径等信息。
- **SkillLoader**: 技能加载工具类，负责从目录中发现和解析技能。
- **AutoBotContextProvider**: 运行时上下文提供器，负责加载技能、生成系统提示和提供工具。
- **SkillsSystemPromptTemplates**: 系统提示模板生成器，生成包含技能信息的系统提示文本。
- **SkillSource**: 枚举类型，区分用户级技能和项目级技能。

## 需求

### 需求 1：简化 RuntimeOptions 中的技能配置

**用户故事：** 作为应用层开发者，我希望通过简单的技能目录路径和技能名称列表来配置技能加载，以便替代当前复杂的多目录扫描配置。

#### 验收标准

1. THE RuntimeOptions SHALL contain a `SkillsDirectory` property of type `string?` to specify the single directory containing skill subdirectories
2. THE RuntimeOptions SHALL contain a `SkillNames` property of type `IList<string>` to specify the names of skills to load from the skills directory
3. THE RuntimeOptions SHALL NOT contain the following properties: `AgentName`, `ProjectRoot`, `EnableUserSkills`, `EnableProjectSkills`, `UserSkillsDirectoryOverride`, `ProjectSkillsDirectoryOverride`, `RefreshIntervalSeconds`
4. THE RuntimeOptions SHALL retain all existing non-skill properties: `WorkingDirectory`, tool enable flags, resource limits, and `ExecutionPolicy`

### 需求 2：按名称加载指定技能

**用户故事：** 作为应用层开发者，我希望运行时仅加载我指定的技能，以便精确控制可用技能集合。

#### 验收标准

1. WHEN `SkillsDirectory` and `SkillNames` are both configured, THE AutoBotContextProvider SHALL use SkillLoader to load only the skills whose names appear in `SkillNames` from the `SkillsDirectory`
2. WHEN `SkillsDirectory` is null or empty, THE AutoBotContextProvider SHALL operate with zero available skills
3. WHEN `SkillNames` is empty, THE AutoBotContextProvider SHALL operate with zero available skills
4. WHEN a skill name in `SkillNames` does not correspond to a valid skill subdirectory, THE AutoBotContextProvider SHALL skip that name and continue loading remaining skills

### 需求 3：简化 SkillsState 为扁平技能容器

**用户故事：** 作为运行时维护者，我希望 SkillsState 持有一个扁平的技能列表，以便消除不再需要的 User/Project 分类和去重逻辑。

#### 验收标准

1. THE SkillsState SHALL hold a single flat collection of SkillMetadata named `Skills`
2. WHEN querying all skills, THE SkillsState SHALL return the flat `Skills` collection directly without deduplication logic
3. WHEN looking up a skill by name, THE SkillsState SHALL perform a case-insensitive match against the flat `Skills` collection and return the first match

### 需求 4：简化系统提示生成

**用户故事：** 作为运行时维护者，我希望系统提示仅展示已注册的技能列表，以便移除对多目录路径配置的依赖。

#### 验收标准

1. WHEN generating the system prompt, THE SkillsSystemPromptTemplates SHALL display only the registered skill names and descriptions
2. WHEN generating the system prompt, THE SkillsSystemPromptTemplates SHALL NOT reference any User/Project skill directory paths or location categories
3. WHEN no skills are registered, THE SkillsSystemPromptTemplates SHALL display a message indicating no skills are available
4. THE SkillsSystemPromptTemplates.GenerateSystemPrompt SHALL accept SkillsState as its sole parameter without requiring RuntimeOptions

### 需求 5：移除 SkillSource 枚举

**用户故事：** 作为运行时维护者，我希望完全移除 SkillSource 枚举，以便消除不再需要的 User/Project 来源区分概念。

#### 验收标准

1. THE SkillSource enum SHALL be removed from the codebase
2. THE SkillMetadata record SHALL NOT contain a `Source` parameter
3. THE SkillLoader and SkillParser SHALL NOT reference SkillSource in their method signatures or internal logic

### 需求 6：确保 ReadSkillTool 兼容新架构

**用户故事：** 作为运行时用户，我希望 read_skill 工具在新架构下正常工作，以便继续通过工具读取技能内容。

#### 验收标准

1. WHEN a user calls read_skill with a valid skill name, THE ReadSkillTool SHALL return the skill content from the simplified SkillsState
2. WHEN a user calls read_skill with a non-existent skill name, THE ReadSkillTool SHALL return an error message indicating the skill was not found

### 需求 7：确保序列化/反序列化兼容性

**用户故事：** 作为运行时维护者，我希望会话恢复功能在新架构下正常工作，以便支持 AutoBotContextProvider 的状态持久化。

#### 验收标准

1. WHEN serializing AutoBotContextProvider state, THE AutoBotContextProvider SHALL serialize RuntimeOptions and the simplified SkillsState
2. WHEN deserializing AutoBotContextProvider state, THE AutoBotContextProvider SHALL restore RuntimeOptions and SkillsState from the serialized data
