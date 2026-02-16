# 需求文档

## 简介

AGUI Web Chat 是一个基于浏览器的 AI 对话客户端，使用 AG-UI 协议（`@ag-ui/client`）连接 AI Agent 后端。用户可以通过 Web 界面发送消息并接收流式 AI 响应，支持文本消息流式传输、工具调用展示、错误处理等功能。

## 术语表

- **Chat_Client**: 基于 React 的 Web 对话客户端应用
- **HttpAgent**: `@ag-ui/client` 提供的 HTTP Agent 类，负责与后端通信
- **Message**: AG-UI 协议中的消息对象，包含 `id`、`role`、`content` 等字段
- **AgentSubscriber**: AG-UI 协议中的事件订阅接口，用于处理流式事件回调
- **Streaming_Response**: Agent 后端通过 SSE 流式返回的文本内容
- **Tool_Call**: Agent 在响应过程中调用的工具，包含工具名称和参数
- **Message_List**: 当前对话中所有消息的有序集合
- **Input_Field**: 用户输入消息的文本输入框组件

## 需求

### 需求 1：发送用户消息

**用户故事：** 作为用户，我希望能够输入文本消息并发送给 AI Agent，以便开始或继续对话。

#### 验收标准

1. WHEN 用户在 Input_Field 中输入文本并按下 Enter 键或点击发送按钮，THEN Chat_Client SHALL 创建一条 role 为 "user" 的 Message 并将其添加到 Message_List 中
2. WHEN 用户尝试发送空白消息（仅包含空格、换行等空白字符），THEN Chat_Client SHALL 阻止发送并保持当前状态不变
3. WHEN 一条用户消息成功添加到 Message_List 后，THEN Chat_Client SHALL 清空 Input_Field 的内容

### 需求 2：接收流式 AI 响应

**用户故事：** 作为用户，我希望能够实时看到 AI Agent 的流式响应内容，以便获得即时反馈体验。

#### 验收标准

1. WHEN 用户消息发送后，THEN Chat_Client SHALL 调用 HttpAgent 的 runAgent 方法发起请求
2. WHEN HttpAgent 接收到 TEXT_MESSAGE_START 事件，THEN Chat_Client SHALL 在 Message_List 中创建一条新的 assistant 消息
3. WHEN HttpAgent 接收到 TEXT_MESSAGE_CONTENT 事件，THEN Chat_Client SHALL 将内容增量追加到当前 assistant 消息中并实时更新显示
4. WHEN HttpAgent 接收到 TEXT_MESSAGE_END 事件，THEN Chat_Client SHALL 标记当前 assistant 消息为完成状态
5. WHILE HttpAgent 正在运行（isRunning 为 true），THEN Chat_Client SHALL 禁用 Input_Field 和发送按钮以防止重复发送

### 需求 3：对话消息展示

**用户故事：** 作为用户，我希望对话消息以清晰的聊天界面展示，以便区分自己的消息和 AI 的回复。

#### 验收标准

1. THE Chat_Client SHALL 将用户消息和 assistant 消息以不同的视觉样式展示（用户消息靠右，assistant 消息靠左）
2. WHEN Message_List 更新时，THEN Chat_Client SHALL 自动滚动到最新消息的位置
3. WHEN assistant 消息包含 Markdown 格式的文本内容，THEN Chat_Client SHALL 将其渲染为格式化的富文本（包括代码块、列表、加粗等）
4. THE Chat_Client SHALL 为每条消息显示对应的角色标识（用户或 AI）

### 需求 4：工具调用展示

**用户故事：** 作为用户，我希望能够看到 AI Agent 在响应过程中调用了哪些工具，以便了解 AI 的推理过程。

#### 验收标准

1. WHEN HttpAgent 接收到 TOOL_CALL_START 事件，THEN Chat_Client SHALL 在界面中展示工具调用的名称
2. WHEN HttpAgent 接收到 TOOL_CALL_ARGS 事件，THEN Chat_Client SHALL 展示工具调用的参数内容
3. WHEN HttpAgent 接收到 TOOL_CALL_END 事件，THEN Chat_Client SHALL 标记该工具调用为完成状态

### 需求 5：错误处理

**用户故事：** 作为用户，我希望在出现错误时能够看到清晰的错误提示，以便了解问题并采取相应措施。

#### 验收标准

1. IF HttpAgent 接收到 RUN_ERROR 事件，THEN Chat_Client SHALL 在对话界面中显示错误提示信息
2. IF 网络连接失败或请求超时，THEN Chat_Client SHALL 显示连接错误提示并允许用户重试
3. IF 发生错误，THEN Chat_Client SHALL 重新启用 Input_Field 和发送按钮，允许用户继续操作

### 需求 6：中止正在进行的请求

**用户故事：** 作为用户，我希望能够中止正在进行的 AI 响应，以便在不需要时停止等待。

#### 验收标准

1. WHILE HttpAgent 正在运行，THEN Chat_Client SHALL 显示一个中止按钮
2. WHEN 用户点击中止按钮，THEN Chat_Client SHALL 调用 HttpAgent 的 abortRun 方法终止当前请求
3. WHEN 请求被中止后，THEN Chat_Client SHALL 重新启用 Input_Field 和发送按钮

### 需求 7：对话历史管理

**用户故事：** 作为用户，我希望能够开始新的对话，以便在不同话题之间切换。

#### 验收标准

1. THE Chat_Client SHALL 提供一个"新对话"按钮
2. WHEN 用户点击"新对话"按钮，THEN Chat_Client SHALL 清空 Message_List 并重置 HttpAgent 的状态
3. WHEN 页面首次加载时，THEN Chat_Client SHALL 显示一个空的对话界面和欢迎提示
