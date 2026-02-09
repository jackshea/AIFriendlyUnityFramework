这是一个为 **AI 原生开发**量身定制的 Unity 游戏框架设计文档。该框架的核心目标是消除 AI（如 Claude Code）与游戏运行状态之间的“黑盒”隔阂，实现逻辑的高度可预测性与自动化测试。

---

# 项目开发文档：AI-Native Logic-First Framework (ALF)

## 1. 架构总览 (Architecture Overview)

框架采用 **MVVM** 模式，结合 **Data Store** 为核心的单向数据流。逻辑层完全脱离 `UnityEngine`，确保 AI 可以在纯 C# 环境中进行 TDD。

### 1.1 三层结构

* **Model (Data Store):** 纯 POCO 类。禁止包含逻辑，仅作为状态（State）的容器。
* **ViewModel/Logic:** 接收 Command，修改 Data Store，并通过响应式属性通知 View。
* **View (Presentation):** 仅包含 `MonoBehaviour`，负责显示模型、粒子、UI，不存储核心状态。

---

## 2. 核心系统设计 (Core Systems)

### 2.1 确定性逻辑步进 (Deterministic Tick System)

为了摆脱 `Time.deltaTime` 带来的不可测性，逻辑层使用固定的 **Tick** 驱动。

* **LogicTick:** 一个纯 C# 的循环，可以在单元测试中手动触发 `Tick()`，模拟游戏运行。
* **Command Pattern:** 所有的交互（攻击、移动）必须封装为 `ICommand`。
* `Execute(GameState state)`: 修改数据状态。
* **AI 友好性:** Command 是序列化的，Claude 可以查看历史记录（Command Log）来回溯逻辑。



### 2.2 数据中心 (Data Store & SSoT)

* **RootState:** 整个游戏只有一个根状态对象。
* **ReactiveProperty:** 使用 `UniRx` 或轻量级响应式包装，使 View 能自动同步数据。
* **自描述元数据:** ```csharp
public class PlayerData {
[Description("当前生命值，取值 0-100")]
public ReactiveProperty<int> HP = new (100);
}
```


```



---

## 3. AI 桥接器：状态内省与指令 (AI Bridge)

这是该框架的灵魂，专为 Claude Code 优化。

### 3.1 状态快照 (State Snapshot)

* **JSON 导出:** 提供 `DebugStateProvider`，可一键将整个 `RootState` 序列化。
* **语义字典:** 自动生成字段说明文档（Schema），告诉 Claude 每个数值的业务含义。

### 3.2 运行时指令控制台 (Runtime Console)

* **Text-Based API:** 集成一个文本控制台。
* **Claude 交互流:**
1. Claude 执行 `get_world_state`。
2. 框架返回结构化 JSON。
3. Claude 分析后执行 `send_command {"type": "UseSkill", "id": 101}`。



### 3.3 逻辑回溯 (Rewind & Diff)

* 记录每个 Tick 的状态快照。当 Bug 发生时，Claude 可以对比 `Tick_N` 和 `Tick_N-1` 的数据差异，定位异常变量。

---

## 4. 技术规格与 AOT 处理 (Technical Specs)

### 4.1 依赖注入 (VContainer)

* **作用域:** 分为 `ProjectLifetimeScope` (全局配置) 和 `GameLifetimeScope` (战斗逻辑)。
* **AOT 优化:** 避免在运行时使用 `System.Reflection.Emit`。所有需要注入的类在 `LifetimeScope` 中显式注册。

### 4.2 IL2CPP / AOT 策略

* **预生成代码:** 使用 C# Source Generators 或在 Editor 下生成静态 AOT 映射表。
* **泛型限制:** 避免深度嵌套的泛型，确保在 IL2CPP 打包时不会丢失元数据。

---

## 5. TDD 开发工作流 (TDD Workflow)

这是针对 Claude Code 优化的标准流程：

| 阶段 | 动作 | 产出 |
| --- | --- | --- |
| **1. 协议定义** | 在纯 C# 程序集 (.asmdef) 中定义 Data 类和接口 | `IDomainLogic.cs`, `GameState.cs` |
| **2. AI 编写测试** | 告诉 Claude 业务规则，让其编写 NUnit 单元测试 | `BattleSystemTests.cs` (全是红灯) |
| **3. 逻辑实现** | Claude 填充 Domain 层代码实现接口 | `BattleSystem.cs` (测试变绿) |
| **4. 表现层绑定** | 在 Unity 中创建 ViewModel 绑定器，连接 Prefab | `PlayerView.cs` |

---

## 6. 项目目录结构建议

```text
Assets/
  ├── _Project/
  │    ├── Scripts/
  │    │    ├── Domain/ (纯C#, 无Unity依赖, TDD核心)
  │    │    │    ├── Models/
  │    │    │    ├── Systems/
  │    │    │    └── Commands/
  │    │    ├── Infrastructure/ (DI注册, AOT辅助)
  │    │    ├── Presentation/ (MonoBehaviours, UI, VFX)
  │    │    └── ViewModels/ (数据转换与桥接)
  │    └── Tests/
  │         ├── Editor/ (纯逻辑单元测试)
  │         └── Runtime/ (集成测试)

```

---

## 7. 给 Claude Code 的第一条指令建议

当你开始开发时，请将以下提示词输入给 Claude：

> "请阅读我们的架构文档。我们现在要从 0 开始构建 `Domain` 层。
> 1. 请先在 `Domain` 程序集下创建一个简单的 `PlayerState` POCO 类。
> 2. 为我写一个 NUnit 测试，验证当 `TakeDamageCommand` 执行时，HP 是否正确扣除且不为负数。
> 3. 记住，不准使用 `UnityEngine.Time` 或 `MonoBehaviour`。"
> 
> 

---

**下一步建议：**
你想让我为你生成这个框架的基础基类代码（如 `ICommand` 接口和 `GameState` 根类），还是先配置 `VContainer` 的基础模板？