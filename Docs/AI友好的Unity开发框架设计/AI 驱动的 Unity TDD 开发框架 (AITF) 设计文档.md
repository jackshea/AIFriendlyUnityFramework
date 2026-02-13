# AI 驱动的 Unity TDD 开发框架 (AITF) 设计文档

## 1. 核心架构逻辑：双环驱动

该框架由两个核心循环组成：

1. **内环 (Logic Loop)**：Claude Code 在外部文件系统中通过 C# 单元测试驱动纯逻辑代码（POCOs）。
2. **外环 (Runtime Loop)**：通过自定义的 **AI Bridge** 插件，让 Claude Code 能够触发 Unity 编辑器内的集成测试并获取实时快照。

------

## 2. 框架分层设计 (Layered Architecture)

为了让 AI 能够高效编写测试，必须强制进行**逻辑与表现分离**。

| **层级**                      | **技术栈**                | **AI 的职责**                                                |
| ----------------------------- | ------------------------- | ------------------------------------------------------------ |
| **Domain (领域层)**           | 纯 C# (No Unity API)      | 编写核心算法、数值计算、游戏规则。AI 对此层拥有 100% 自动化测试能力。 |
| **Infrastructure (基础设施)** | VContainer, Interfaces    | 定义存储、网络、输入的接口协议。                             |
| **Presentation (表现层)**     | MonoBehaviour, UI Toolkit | 仅负责渲染和输入转发。AI 通过“状态快照”观察此层。            |
| **AI Bridge (通讯层)**        | CLI / WebSocket / JSON    | 将 Unity 运行状态（报错、变量、层级）翻译给 Claude。         |

------

## 3. AI 友好的 TDD 工作流设计

### 第一阶段：契约定义 (Contract)

用户通过 Claude Code 定义功能接口。

- **动作**：Claude 生成 `IMovementSystem.cs`。
- **要求**：必须包含 XML 文档注释，以便 Claude 后续查阅。

### 第二阶段：编写“红灯”测试 (Red Test)

在代码实现之前，Claude 必须在 `Tests/Editor` 下生成 NUnit 测试。

- **指令示例**：`"根据 IMovementSystem 接口，写一个测试用例：当速度为 5 时，移动 2 秒后位移应为 10。"`

### 第三阶段：自动化测试执行器 (Test Runner CLI)

这是实现高自动化的关键。你需要一个命令行工具让 Claude 能直接运行测试：

Bash

```
# Claude Code 可以在终端执行的指令示例
unity-tester --run-mode Editor --filter MovementTests
```

- **设计实现**：利用 Unity 的 `-runTests` 命令行参数，并编写一个 Python/Node.js 包装脚本，将结果从 XML 转换为更易读的 Markdown 摘要返回给 Claude。

------

## 4. 实时状态监控系统 (AI-Eyes)

为了让 AI 了解运行状态，框架需内置 **State Snapshotter**。

### 4.1 自动错误报告 (Self-Healing)

当 Unity 发生运行时错误时，框架自动在项目根目录生成 `LAST_ERROR.json`：

JSON

```
{
  "exception": "NullReferenceException",
  "stackTrace": "at PlayerController.Update()...",
  "context": {
    "activeScene": "BattleScene",
    "selectedObject": "Player",
    "relevantVariables": { "health": 100, "weapon": null }
  }
}
```

**自动化逻辑**：Claude Code 监控此文件，一旦更新，自动进入修复模式。

### 4.2 场景结构描述符 (Scene To Markdown)

编写一个 Unity 工具，将当前 Hierarchy 转换为树状 Markdown：

- **功能**：Claude 调用指令 `get-scene-tree`。
- **输出**：
  - `[GO] Player (Pos: 0,0,0)`
    - `[Comp] PlayerController (Speed: 5)`
  - `[GO] Enemy_Manager`

------

## 5. 环境配置文件 (`.clauderc` / `AI_GUIDE.md`)

在根目录放置此文件，作为 Claude 的“操作系统说明书”。

Markdown

```
# Unity AI 开发规范
1. **测试先行**：所有 Feature 必须先在 `Tests/` 下有对应的测试文件。
2. **禁止单例**：使用 `VContainer` 进行依赖注入，入口点在 `GameLifetimeScope.cs`。
3. **状态获取**：如果需要查看运行时状态，请运行命令 `npm run unity-snapshot`。
4. **修复逻辑**：遇到 Console 报错，请先读取 `LAST_ERROR.json`。
5. **代码风格**：每个类不超过 150 行。如果超过，请重构为多个子系统。
```

------

## 6. 核心组件代码示例 (AI Bridge 雏形)

### 6.1 结构化日志包装器

C#

```
public static class AILogger {
    public static void LogState(string module, object data) {
        // 将状态序列化为 JSON 写入特定临时文件，供 Claude 读取
        string json = JsonUtility.ToJson(data);
        File.WriteAllText($"{Application.dataPath}/../AI_Context/{module}.json", json);
    }
}
```

### 6.2 自动化测试触发脚本 (Node.js)

JavaScript

```
// scripts/run-tests.js
const { exec } = require('child_process');
// 调用 Unity 命令行运行测试
exec('Unity.exe -runTests -batchmode -projectPath ...', (err, stdout, stderr) => {
    const results = parseXML(stdout); // 解析测试结果
    console.log(formatForClaude(results)); // 以友好格式输出给 Claude
});
```

------

## 7. 下一步行动建议

要实现这套方案，我建议您分三步走：

1. **基础设施搭建**：在 Unity 中安装 **VContainer**（用于解耦）和 **Unity Test Framework**。
2. **建立通讯桥梁**：编写一个简单的脚本，能将 `Debug.Log` 实时同步到一个文本文件中，让 Claude Code 可以通过 `tail -f` 观察。
3. **编写第一个 AI TDD 模块**：尝试让 Claude 写一个不依赖 Unity API 的计算逻辑（如：经验值升级算法），并配置好命令行测试运行。

**您想让我为您先写出那个能让 Claude 一键运行 Unity 测试并获取结果的外部脚本（Node.js/Python），还是先写 Unity 内部的“场景快照”导出工具？**