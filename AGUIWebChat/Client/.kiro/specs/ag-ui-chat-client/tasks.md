# 实现计划：AG-UI 对话客户端

## 概述

基于设计文档，将实现分为：项目基础设施搭建 → 核心 Hook 实现 → UI 组件实现 → 页面集成 → 测试。每个步骤递增构建，确保无孤立代码。

## 任务

- [x] 1. 安装依赖并搭建项目基础设施
  - [x] 1.1 安装新依赖：`react-markdown`、`remark-gfm`、`tailwind-merge`、`clsx`、`class-variance-authority`、`@radix-ui/react-collapsible`、`@radix-ui/react-scroll-area`、`fast-check`
    - 运行 `pnpm add react-markdown remark-gfm tailwind-merge clsx class-variance-authority @radix-ui/react-collapsible @radix-ui/react-scroll-area`
    - 运行 `pnpm add -D fast-check`
  - [x] 1.2 创建 `src/lib/utils.ts`，实现 shadcn/ui 的 `cn()` 工具函数
    - 使用 `clsx` + `tailwind-merge` 组合
    - _Requirements: 无（基础设施）_
  - [x] 1.3 创建 shadcn/ui 基础组件：`src/components/ui/button.tsx`、`textarea.tsx`、`scroll-area.tsx`、`card.tsx`、`collapsible.tsx`
    - 按照 shadcn/ui 标准模式实现，使用 `class-variance-authority` 定义变体
    - _Requirements: 无（基础设施）_

- [x] 2. 实现数据模型和核心 Hook
  - [x] 2.1 在 `src/hooks/useChat.ts` 中定义 `ChatMessage` 和 `ChatToolCall` 类型接口
    - 包含 `id`、`role`、`content`、`isStreaming`、`toolCalls`、`isError` 字段
    - _Requirements: 3.1, 3.4_
  - [x] 2.2 实现 `useChat` Hook 的基础结构
    - 使用 `useRef` 持有 `HttpAgent` 实例（url: `http://localhost:5050`）
    - 使用 `useState` 管理 `messages`、`isRunning`、`error` 状态
    - 导出 `UseChatReturn` 接口：`messages`、`isRunning`、`error`、`sendMessage`、`abortRun`、`clearChat`
    - _Requirements: 2.1, 7.2_
  - [x] 2.3 实现 `sendMessage` 函数
    - 验证输入（trim 后非空）
    - 创建 user 消息并添加到 messages 状态
    - 调用 `agent.runAgent()` 并传入 `AgentSubscriber`
    - _Requirements: 1.1, 1.2, 1.3, 2.1_
  - [x] 2.4 实现 `AgentSubscriber` 事件处理
    - `onTextMessageStartEvent`：创建新的 assistant 消息（isStreaming: true）
    - `onTextMessageContentEvent`：追加文本内容到当前 assistant 消息
    - `onTextMessageEndEvent`：标记 isStreaming 为 false
    - `onToolCallStartEvent`：添加工具调用记录
    - `onToolCallArgsEvent`：追加工具调用参数
    - `onToolCallEndEvent`：标记工具调用完成
    - `onRunErrorEvent`：设置 error 状态，添加错误消息
    - 运行结束后重置 isRunning 状态
    - _Requirements: 2.2, 2.3, 2.4, 2.5, 4.1, 4.2, 4.3, 5.1_
  - [x] 2.5 实现 `abortRun` 和 `clearChat` 函数
    - `abortRun`：调用 `agent.abortRun()`，重置 isRunning
    - `clearChat`：清空 messages，重置 agent 状态
    - _Requirements: 6.2, 6.3, 7.2_
  - [ ]* 2.6 编写 `useChat` Hook 的属性测试
    - **Property 1: 发送有效消息增长消息列表**
    - **Validates: Requirements 1.1**
    - **Property 2: 纯空白输入被拒绝**
    - **Validates: Requirements 1.2**
  - [ ]* 2.7 编写 `useChat` Hook 的属性测试（流式累积）
    - **Property 4: 流式文本内容累积正确性**
    - **Validates: Requirements 2.3**
    - **Property 7: 工具调用参数累积正确性**
    - **Validates: Requirements 4.2**

- [x] 3. 检查点 - 确保核心 Hook 测试通过
  - 确保所有测试通过，如有问题请询问用户。

- [x] 4. 实现 UI 组件
  - [x] 4.1 实现 `src/components/MarkdownRenderer.tsx`
    - 封装 `react-markdown`，配置 `remark-gfm` 插件
    - 自定义代码块渲染（添加语言类名和 Tailwind 样式）
    - 自定义链接渲染（`target="_blank"`）
    - _Requirements: 3.3_
  - [x] 4.2 实现 `src/components/MessageBubble.tsx`
    - 根据 role 决定对齐方向（user 靠右，assistant 靠左）
    - user 消息使用主题色背景，assistant 消息使用 Card 组件
    - assistant 消息通过 MarkdownRenderer 渲染内容
    - 使用 lucide-react 图标（User / Bot）显示角色标识
    - 包含 ToolCallDisplay 渲染工具调用
    - _Requirements: 3.1, 3.3, 3.4, 4.1, 4.2, 4.3_
  - [x] 4.3 实现 `src/components/ToolCallDisplay.tsx`
    - 使用 Collapsible 组件实现可折叠/展开
    - 展示工具名称和参数内容
    - _Requirements: 4.1, 4.2, 4.3_
  - [x] 4.4 实现 `src/components/ChatMessageList.tsx`
    - 使用 ScrollArea 组件包裹消息列表
    - 遍历 messages 渲染 MessageBubble
    - 使用 useRef + useEffect 实现自动滚动到底部
    - _Requirements: 3.1, 3.2_
  - [x] 4.5 实现 `src/components/ChatInput.tsx`
    - 使用 shadcn/ui Textarea 和 Button 组件
    - Enter 发送（Shift+Enter 换行），空白内容禁用发送
    - 运行中显示中止按钮（Square 图标），空闲时显示发送按钮（SendHorizontal 图标）
    - isRunning 时禁用输入框和发送按钮
    - _Requirements: 1.1, 1.2, 1.3, 2.5, 6.1, 6.2_
  - [ ]* 4.6 编写 UI 组件单元测试
    - 测试 ChatInput 的发送和中止交互
    - 测试 MessageBubble 的角色样式区分
    - 测试运行中 UI 状态（Property 5: 运行中 UI 状态一致性）
    - **Validates: Requirements 2.5, 6.1**
    - 测试运行终止后 UI 恢复（Property 6: 运行终止后 UI 恢复）
    - **Validates: Requirements 5.3, 6.3**

- [x] 5. 集成页面路由
  - [x] 5.1 重写 `src/routes/index.tsx` 为 ChatPage
    - 调用 `useChat` Hook
    - 组合 ChatHeader（新对话按钮）、ChatMessageList、ChatInput
    - 首次加载显示欢迎提示
    - 错误状态显示错误提示
    - _Requirements: 5.1, 5.2, 7.1, 7.2, 7.3_
  - [ ]* 5.2 编写页面集成单元测试
    - 测试初始状态显示欢迎提示
    - 测试新对话按钮清空消息
    - 测试错误提示显示
    - _Requirements: 5.1, 5.2, 7.1, 7.2, 7.3_

- [x] 6. 最终检查点 - 确保所有测试通过
  - 确保所有测试通过，如有问题请询问用户。

## 备注

- 标记 `*` 的任务为可选任务，可跳过以加速 MVP 开发
- 每个任务引用了具体的需求编号以确保可追溯性
- 检查点确保增量验证
- 属性测试验证通用正确性属性，单元测试验证具体示例和边界情况
