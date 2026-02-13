# AI 诊断与指令桥接系统

要提升 Claude Code 或 AI Agent 对 Unity 运行状态的感知并增强其自动化能力，核心在于**将原本“黑盒”的游戏运行状态转化为 AI 可读、可理解、可操作的结构化数据**。

你可以通过设计一个 **“AI 诊断与指令桥接系统” (AI Bridge)** 来实现这一目标。

------

## 1. 运行时状态观察器 (The AI's "Eyes")

AI 无法直接观察屏幕，但它可以读取文本。你需要一个工具将 Unity 的层级结构和内存状态“快照化”。

### 结构化层级导出 (Hierarchy to JSON)

编写一个编辑器脚本，允许 AI 通过命令行触发，将当前场景的 `Hierarchy` 导出为精简的 JSON 或 Markdown。

- **设计点**：不要导出所有对象，只导出带有特定组件（如 `IAIInteractable`）或关键路径的对象。

- **反馈示例**：

  JSON

  ```
  {
    "Scene": "Level_01",
    "ActiveActors": [
      {"id": 102, "name": "Player", "pos": [0,1,0], "hp": 85, "state": "Idle"},
      {"id": 501, "name": "Goblin_A", "pos": [5,0,5], "hp": 0, "state": "Dead"}
    ]
  }
  ```

### 运行时反射检查器

允许 AI 发送查询请求，获取特定对象的私有变量值。

- **实现**：建立一个基于反射的 `ReflectionService`，AI 可以输入 `GetField("Player", "_moveSpeed")`，系统返回实时数值。

------

## 2. 实时指令与控制台 (The AI's "Hands")

要让 AI 能够修改运行中的游戏，你需要一个比 Unity 自带 Inspector 更“自动化”的接口。

### 扩展型远程控制台 (Remote Console)

利用 **WebSocket** 或简单的 **Local Web Server**（在 Unity Editor 内运行），让 Claude Code 可以通过终端指令直接改变游戏状态。

- **指令化操作**：AI 可以执行类似 `cmd --call "Player.Heal(20)"` 或 `cmd --set-time-scale 0.5` 的命令。
- **热重载配置**：将游戏平衡参数（如速度、攻击力）放在 JSON 中。AI 修改 JSON 后，框架通过 `FileWatcher` 自动触发游戏内的参数更新，实现“边跑边调”。

------

## 3. 结构化日志系统 (The AI's "Ears")

传统的 `Debug.Log` 充满了杂音。AI 需要的是带上下文、可过滤的结构化诊断信息。

### 语义化日志 (Semantic Logging)

创建一个包装类 `AILogger`，每条日志都包含特定的 **标签 (Tags)** 和 **重要等级 (Severity)**。

- **设计示例**：

  > `[COMBAT][HIGH] Player attacked Goblin_A, Damage: 15, Enemy HP remaining: 0.`

- **自动化反馈循环**：

  当发生 `Error` 或 `Exception` 时，框架自动将报错行代码、附近 10 行的上下文以及当前的内存快照打包成一个 `Issue_Report.md`。Claude Code 读取此文件后即可直接开始修复逻辑。

------

## 4. 自动化测试与验证闭环 (The Feedback Loop)

这是实现高自动化程度的关键：**AI 编写代码 -> AI 触发测试 -> 框架返回运行结果 -> AI 根据结果修正。**

### 运行时冒烟测试自动化

设计一个通用的测试运行器，AI 可以通过命令行启动它：

1. **AI 生成代码**：修改了角色跳跃逻辑。
2. **AI 触发测试**：通过 CLI 调用 Unity `BatchMode` 运行 `Test_Player_Jump` 单元测试。
3. **结果反馈**：测试框架返回“Jump height reached: 1.2m (Expected: 1.5m)”。
4. **AI 自我修正**：Claude 看到反馈后，自动调整 `JumpForce` 参数并重新测试。

------

## 5. 框架集成建议：AI Sidecar 模式

为了不污染生产代码，建议采用 **Sidecar (边车) 模式**：

1. **AI-Agent-Toolkit (Unity Package)**：
   - 在开发环境下挂载一个隐藏的 `GameObject`，负责所有的 WebSocket 通信和状态监控。
   - **不要**将这些代码打包进正式版（使用 `#if UNITY_EDITOR`）。
2. **CLI 桥接脚本 (Node.js/Python)**：
   - 编写一个简单的中间件，连接 Claude Code 的终端和 Unity 的 WebSocket。
   - Claude 通过执行 `./tell-unity "spawn enemy"`，中间件转发给 Unity，Unity 实时生成敌人并返回成功消息。

------

### 总结：AI 友好的“全栈”交互流程

| **维度** | **传统开发**           | **AI 友好型开发**                       |
| -------- | ---------------------- | --------------------------------------- |
| **感知** | 人眼看 Game 窗口       | AI 读取结构化状态快照 (JSON/Markdown)   |
| **操作** | 鼠标手动调节 Inspector | AI 通过 CLI 发送反射指令或修改热载配置  |
| **调试** | 手动打断点观察变量     | AI 订阅结构化语义日志，接收自动错误报告 |
| **验证** | 手动运行游戏测试       | AI 触发自动化单元测试并根据数值反馈修正 |

**接下来，您希望我为您展示如何编写一个“Hierarchy-to-JSON”导出脚本，还是演示如何搭建那个用于通信的本地 WebSocket 服务？**