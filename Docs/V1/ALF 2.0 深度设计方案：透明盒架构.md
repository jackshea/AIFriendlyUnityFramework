# ALF 2.0 深度设计方案：透明盒架构

## 1. 核心设计哲学升级：从“分离”到“镜像”

原文档提到了 MVVM 和逻辑分离，2.0 方案建议引入 **"Shadow Simulation"（影子模拟）** 概念。

- **现状：** 逻辑层控制数据，Unity 层表现数据。
- **问题：** 很多逻辑依赖空间关系（如：“寻找最近的敌人”）。如果完全脱离 Unity Transform，AI 写的纯 C# 逻辑无法感知空间。
- **解决方案：** 在 Domain 层引入 **"Pure C# Spatial Graph"**。
  - 逻辑层不持有 `Transform`，但持有 `Vector3` 和 `BoundingBox` 数据。
  - **SpatialSystem (Pure C#):** 使用简单的空间哈希或四叉树在纯逻辑层处理空间查询。
  - **Sync:** Unity 的 `Update` 负责将物理引擎的结果（如果使用了 Unity Physics）回写到 Domain 层的坐标数据中，或者反之（如果是逻辑驱动运动）。

------

## 2. 数据架构深化：Context-Aware State (上下文感知状态)

为了解决 Claude 上下文窗口限制的问题，不能每次都 dump 整个 `RootState`。我们需要实现 **"切片式" (Slicing)** 的状态查询。

### 2.1 状态切片 (State Slicing) 机制

设计一个 `StateQuery` 接口，允许 AI 只获取当前任务相关的数据。

- **Query DSL:** 设计一套简单的查询语法。
  - `GET /state/player?depth=1` (只看玩家基础数据)
  - `GET /state/nearby_enemies?radius=10` (空间查询)
  - `GET /history/commands?limit=5` (查看最近5步操作)
- **AI 价值:** 当 Claude 调试 "背包系统" 时，它不需要看到 "地图地形数据"。这极大地减少了 Token 消耗并提高了 AI 的注意力准确度。

### 2.2 契约式数据 (Contract-Based Data)

在 POCO 类中引入**运行时校验元数据**。

C#

```
// 伪代码设计概念
public class PlayerState {
    // 契约：AI 在编写逻辑时，如果违反此约束，框架会直接抛出明确的英文错误给 AI
    [Invariant("HP >= 0 && HP <= MaxHP")] 
    public ReactiveProperty<int> HP;
    
    [Invariant("Inventory.Count <= MaxSlots")]
    public List<Item> Inventory;
}
```

**工作流:** 当 AI 生成的代码导致 `HP` 变成 -1 时，框架捕获异常并生成 prompt：`"Error: Invariant violation in PlayerState. HP is -1. Check damage calculation logic."` 这种精准反馈比简单的 `NullReferenceException` 对 AI 更有效。

------

## 3. 逻辑层深化：事件溯源 (Event Sourcing)

为了让 AI 能够完美 Debug，仅仅有 Command 是不够的，我们需要**完全的可重现性**。

### 3.1 确定性时间轴 (Deterministic Timeline)

- **Input Stream:** 玩家的所有输入（键盘、鼠标）不直接修改 State，而是先转化为 `InputFrame` 数据包。
- **RNG Seed:** 随机数生成器必须是注入的，且种子是状态的一部分。
- **Replay System:**
  - **Record:** 记录 `InitialState` + `List<InputFrame>`。
  - **Replay:** 可以在 Editor 中（甚至脱离 Unity 渲染）瞬间跑完 1000 帧逻辑，重现 Bug 现场。

### 3.2 逻辑/表现双层事件总线

- **Domain Events (纯逻辑):** `OnEnemyDied` (C# event)。AI 监听此事件处理掉落。
- **Presentation Events (视觉):** `RequestVFX` (struct)。Unity View 监听此事件播放粒子。
  - **关键点:** View 层只读。View 不允许向 Logic 层发送任何东西，只能发送 User Input Command。

------

## 4. AI 桥接器升级：语义反射 (Semantic Reflection)

这是让 Claude "读懂" 代码的关键。

### 4.1 自动生成 API 文档

框架应包含一个 Editor 工具，利用 Reflection 和 Source Generator，为当前的 Domain 层生成一份 **"AI 阅读手册" (Markdown)**。

- **内容:**
  - 所有可用的 `Command` 列表及其参数说明。
  - 状态树的结构图。
  - 关键系统的依赖关系。
- **用法:** 每次 AI 开启新任务前，自动将此 Markdown 注入到 System Prompt 中。

### 4.2 运行时 "意图测试" (Intent Testing)

允许 AI 在 Console 中不仅发送 Command，还能发送 **Assertion**。

- **Claude 操作流程:**
  1. 发送指令: `execute Command_Attack`
  2. 发送断言: `assert Enemy.HP < Previous.HP`
- **框架反馈:**
  - `Success: Command executed. Assertion passed.`
  - `Failure: Command executed, BUT Enemy.HP did not change. Possible cause: Enemy is invincible.`

------

## 5. 目录结构与模块化建议 (深化版)

为了支持大型项目，建议采用 **Feature-Based** 结构，而不是层级结构。

Plaintext

```
Assets/_Project/
 ├── Core/ (框架底层：Tick, EventBus, BaseClasses)
 ├── Features/ (按业务功能垂直切分)
 │    ├── Combat/
 │    │    ├── Domain/ (纯逻辑：State, Systems, Calculators)
 │    │    ├── View/ (Unity组件：Animators, VFX)
 │    │    ├── Tests/ (该模块的专属测试)
 │    │    └── Combat.asmdef (引用 Core, 无 Unity 引用)
 │    ├── Inventory/
 │    └── Locomotion/
 ├── Bootstrap/ (程序入口，DI 配置)
 └── Simulation/ (纯 C# 模拟运行环境，用于跑 Headless 测试)
```

------

## 6. AI 开发工作流闭环 (The Loop)

设计一个标准的 AI 交互协议：

1. **Define (定义):**
   - 开发者（人）定义 `State` 结构和 `ICommand` 接口。
2. **Test (测试驱动):**
   - 人: "我要一个受到攻击扣血的逻辑。"
   - AI: 生成 `DamageSystemTests.cs` (Red)。
3. **Implement (实现):**
   - AI: 生成 `DamageSystem.cs`。
   - Framework: 自动运行测试。
     - **如果通过:** 提交。
     - **如果失败:** 框架捕获 NUnit 报错信息 -> 自动喂回给 AI -> AI 修正代码 (Self-Healing)。
4. **Visualize (绑定):**
   - 人: 负责在 Unity Editor 中将 View 挂载，并拖拽绑定 ViewModel。

------

## 7. 总结：该方案的核心优势

1. **脱耦:** 彻底将 "怎么算" (Logic) 和 "怎么画" (Unity) 分开，AI 只需要负责 "怎么算"，这是它的强项。
2. **白盒化:** 通过 **State Slicing** 和 **Event Sourcing**，原本不可见的运行时状态变成了可查询的文本。
3. **自愈性:** 结合 TDD 和 契约检查，AI 可以自己通过报错信息修正逻辑，不需要人肉 Debug。

**你需要我针对其中的某一个模块（例如“空间查询的纯逻辑实现”或“自动化测试反馈回路”）提供更具体的接口设计吗？**
