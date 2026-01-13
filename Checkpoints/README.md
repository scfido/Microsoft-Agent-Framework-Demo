# Checkpoint 恢复机制分析

## 运行结果分析

### 第一次执行流程

工作流按顺序执行了三个步骤：

1. **Step1 执行**
   - 输入：`"Checkpoint Workflow"`
   - 输出：`"Checkpoint Workflow -> Step1"`
   - Checkpoint保存：`"Checkpoint Workflow -> Step1"`

2. **Step2 执行**
   - 输入：`"Checkpoint Workflow -> Step1"`
   - 输出：`"Checkpoint Workflow -> Step1 -> Step2"`
   - Checkpoint保存：`"Checkpoint Workflow -> Step1 -> Step2"`

3. **Step3 执行**
   - 输入：`"Checkpoint Workflow -> Step1 -> Step2"`
   - 输出：`"Checkpoint Workflow -> Step1 -> Step2 -> Step3"`
   - Checkpoint保存：`"Checkpoint Workflow -> Step1 -> Step2 -> Step3"`

### 恢复流程（从 Step2 完成后的 Checkpoint 恢复）

恢复时从 `checkpoints[1]`（即 Step2 完成后的 checkpoint）恢复：

1. **状态恢复**
   - `Restore Step3 with state:` （空状态，因为 Step3 尚未执行）
   - `Restore Step2 with state: Checkpoint Workflow -> Step1 -> Step2` （恢复 Step2 的状态）
   - `Restore Step1 with state: Checkpoint Workflow -> Step1` （恢复 Step1 的状态）

2. **执行恢复**
   - 只执行了 `StepExecutor3`
   - 输入：`"Checkpoint Workflow -> Step1 -> Step2"`
   - 输出：`"Checkpoint Workflow -> Step1 -> Step2 -> Step3"`

## 恢复 Checkpoint 对已执行工作流节点的影响

### 核心结论

**恢复 Checkpoint 时，已执行的节点不会重新执行，只会恢复其状态。**

### 详细说明

1. **状态恢复**
   - 所有已执行节点的状态都会通过 `OnCheckpointRestoredAsync` 方法恢复
   - 状态值恢复到 Checkpoint 保存时的值
   - 恢复顺序：从后往前（Step3 → Step2 → Step1）

2. **执行跳过**
   - 已执行的节点（Step1、Step2）不会重新调用 `HandleAsync` 方法
   - 只有未执行的节点（Step3）会继续执行

3. **工作流连续性**
   - 恢复后，工作流从 Checkpoint 保存的位置继续执行
   - 未执行节点的输入来自已执行节点的最终输出状态
   - 保证了工作流的逻辑连续性和数据一致性

### 优势

- **性能优化**：避免重复执行已完成的计算
- **状态一致性**：确保恢复后的状态与 Checkpoint 保存时完全一致
- **资源节约**：只执行必要的后续步骤

### 实现机制

- `OnCheckpointingAsync`：在 SuperStep 完成时保存节点状态
- `OnCheckpointRestoredAsync`：恢复节点状态，但不触发重新执行
- 工作流引擎根据 Checkpoint 信息判断哪些节点已执行，哪些需要继续执行
