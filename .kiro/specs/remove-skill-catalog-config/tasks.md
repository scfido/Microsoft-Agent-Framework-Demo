# 实现计划：移除技能目录配置

## 概述

将 AutoBot 运行时从多目录自动扫描架构改为单一目录按名称加载架构。按照自底向上的顺序修改：先改数据模型和底层组件，再改上层消费者，最后清理和集成。

## Tasks

- [x] 1. 移除 SkillSource 枚举并更新 SkillMetadata
  - [x] 1.1 删除 `SkillSource.cs` 文件，从 `SkillMetadata` record 中移除 `Source` 参数
    - 删除 `AutoBot/SkillEngine/SkillSource.cs`
    - 修改 `AutoBot/SkillEngine/SkillMetadata.cs`：从 record 参数列表中移除 `Source` 参数
    - _Requirements: 5.1, 5.2_
  - [x] 1.2 更新 SkillParser 移除 SkillSource 引用
    - 修改 `AutoBot/SkillEngine/SkillParser.cs`：`Parse` 和 `ParseContent` 方法移除 `SkillSource source` 参数
    - 更新内部 `SkillMetadata` 构造调用，不再传入 `Source`
    - _Requirements: 5.3_
  - [x] 1.3 更新 SkillLoader 移除 SkillSource 引用并新增 LoadSkillByName 方法
    - 修改 `AutoBot/SkillEngine/SkillLoader.cs`：`LoadSkillsFromDirectory` 和 `TryLoadSkill` 方法移除 `SkillSource source` 参数
    - 新增 `LoadSkillByName(string skillsDirectory, string skillName)` 公共方法
    - _Requirements: 5.3, 2.1_

- [x] 2. 简化 SkillsState
  - [x] 2.1 将 SkillsState 改为扁平技能容器
    - 修改 `AutoBot/SkillsState.cs`：移除 `UserSkills`、`ProjectSkills` 属性和 `LastRefreshed` 属性
    - 新增 `Skills` 属性（`IReadOnlyList<SkillMetadata>`）
    - 简化 `AllSkills` 为直接返回 `Skills`
    - 简化 `GetSkill` 为在 `Skills` 上进行大小写不敏感查找
    - _Requirements: 3.1, 3.2, 3.3_
  - [ ]* 2.2 编写 SkillsState 属性测试
    - **Property 2: AllSkills 返回原始列表（无去重）**
    - **Property 3: 技能名称大小写不敏感查找**
    - **Validates: Requirements 3.2, 3.3**

- [x] 3. 简化 RuntimeOptions
  - [x] 3.1 移除旧技能目录配置属性，新增简化配置
    - 修改 `AutoBot/RuntimeOptions.cs`：移除 `AgentName`、`ProjectRoot`、`EnableUserSkills`、`EnableProjectSkills`、`UserSkillsDirectoryOverride`、`ProjectSkillsDirectoryOverride`、`RefreshIntervalSeconds`
    - 新增 `SkillsDirectory`（`string?`）和 `SkillNames`（`IList<string>`）属性
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 4. 简化 SkillsSystemPromptTemplates
  - [x] 4.1 修改系统提示生成逻辑
    - 修改 `AutoBot/SkillsSystemPromptTemplates.cs`：`GenerateSystemPrompt` 方法签名改为仅接受 `SkillsState`，移除 `RuntimeOptions` 参数
    - 从模板中移除 `{skills_locations}` 占位符
    - 删除 `GenerateSkillsLocationsDisplay` 方法
    - 简化 `GenerateSkillsList` 为直接遍历 `state.Skills`，移除 User/Project 分类逻辑
    - _Requirements: 4.1, 4.2, 4.3, 4.4_
  - [ ]* 4.2 编写系统提示属性测试
    - **Property 4: 系统提示包含所有技能信息且不含目录路径**
    - **Validates: Requirements 4.1, 4.2**

- [x] 5. Checkpoint - 确保底层组件编译通过
  - 确保所有底层修改编译通过，如有问题请询问用户。

- [x] 6. 更新 AutoBotContextProvider
  - [x] 6.1 重写 LoadSkills 方法和构造函数
    - 修改 `AutoBot/AutoBotContextProvider.cs`：重写 `LoadSkills()` 方法，从 `_options.SkillsDirectory` 中按 `_options.SkillNames` 逐个调用 `_skillLoader.LoadSkillByName` 加载技能
    - 移除 `GetDefaultUserSkillsDirectory` 和 `GetDefaultProjectSkillsDirectory` 私有方法
    - 更新 `InvokingCoreAsync` 中 `GenerateSystemPrompt` 调用，移除 `_options` 参数
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 7. 更新 ReadSkillTool
  - [x] 7.1 从 JSON 输出中移除 source 字段
    - 修改 `AutoBot/Tools/ReadSkillTool.cs`：从 `ExecuteAsync` 返回的 JSON 对象中移除 `source = skill.Source.ToString()` 字段
    - _Requirements: 6.1, 6.2_

- [x] 8. Final checkpoint - 确保所有修改编译通过并集成正确
  - 确保所有修改编译通过，如有问题请询问用户。

## Notes

- 标记 `*` 的任务为可选任务，可跳过以加快 MVP 进度
- 每个任务引用具体需求以确保可追溯性
- 属性测试验证通用正确性属性，单元测试验证具体示例和边界情况
- 修改顺序为自底向上：先改数据模型（SkillSource、SkillMetadata），再改中间层（SkillsState、RuntimeOptions、SkillsSystemPromptTemplates），最后改上层（AutoBotContextProvider、ReadSkillTool）
