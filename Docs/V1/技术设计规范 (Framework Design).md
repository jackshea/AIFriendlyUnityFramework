# AI 原生逻辑优先框架 (ALF) - 技术设计规范 (Technical Design Specification)

本文档详细阐述了 ALF 架构的技术实现，旨在通过强制分离逻辑（纯 C#）与表现（Unity），最大化 AI 辅助编程（如 Claude Code/Cursor）的效率。

## 1. 核心理念：跨越“AI 鸿沟”
AI 在以下环境下表现最佳：
1.  **上下文小而纯粹：** 不需要读取庞大的 `UnityEngine.dll` 元数据来理解简单的移动逻辑。
2.  **逻辑是确定性的：** 代码 $A(State) + Command \rightarrow State'$ 比依赖 `Update()` 的不确定执行更容易推理。
3.  **反馈是即时的：** <1秒的 TDD（红/绿循环）远优于 10秒的“Play Mode”启动。

## 2. 确定性逻辑步进系统 (Deterministic Tick System, DTS)

旨在将游戏逻辑与 Unity 的可变帧率及 `Time.deltaTime` 解耦。

### 2.1 核心循环 (`TickEngine`)
位于 `Framework.Core`。
一个纯 C# 类，管理固定时间步长的累加器。

```csharp
public class TickEngine {
    private readonly float _fixedDeltaTime;
    private double _accumulator;
    private long _tickCount;
    
    // 逻辑入口点
    public event Action<TickContext> OnTick; 

    public void Update(float unityDeltaTime) {
        _accumulator += unityDeltaTime;
        while (_accumulator >= _fixedDeltaTime) {
            _tickCount++;
            OnTick?.Invoke(new TickContext(_fixedDeltaTime, _tickCount));
            _accumulator -= _fixedDeltaTime;
        }
    }
    
    // 视图插值因子 (alpha = accumulator / fixedDeltaTime)
    public float InterpolationAlpha => (float)(_accumulator / _fixedDeltaTime);
}
```

### 2.2 Unity 驱动器 (`GameLoopDriver`)
位于 `Game.Presentation`。
这是唯一驱动逻辑的 `MonoBehaviour`。

```csharp
public class GameLoopDriver : MonoBehaviour {
    [Inject] private TickEngine _tickEngine;
    
    void Update() {
        // 将 Unity 的时间喂给逻辑引擎
        _tickEngine.Update(Time.deltaTime); 
    }
}
```

## 3. 响应式数据仓库 (Reactive Data Store)

一个集中的、可序列化的状态树。逻辑层处理 **Command（指令）**，数据层通过 **Event（事件）** 做出反应。

### 3.1 响应式属性 (`BindableProperty<T>`)
值类型的轻量级包装器，用于检测变化。
*   **为什么不用纯 UniRx？** 我们需要更简单的序列化以及对 AI 更友好的状态检查。
*   **特性：** `Version` 计数器，帮助 AI 识别过时数据。

```csharp
public class BindableProperty<T> {
    public T Value { 
        get => _value;
        set {
            if (!Equals(_value, value)) {
                _value = value;
                Version++;
                OnValueChanged?.Invoke(value);
            }
        }
    }
    public int Version { get; private set; }
}
```

### 3.2 状态 Schema 示例
```csharp
// Game.Domain/Models/GameState.cs
public class GameState {
    public ReactiveDictionary<int, EntityState> Entities = new();
    public BindableProperty<float> GlobalTime = new();
    // ...
}
```

## 4. AI 桥接与内省 (AI Bridge & Introspection)

这是 AI (Claude) 与运行中游戏对话的“API”。

### 4.1 Schema 导出器 (SchemaDumper)
反射扫描 `Game.Domain` 并生成 `schema.md` 的工具。
*   **输出：** 列出所有 `ICommand` 类型及其字段。
*   **用途：** AI 读取 `schema.md` 后，就知道它可以发送 `MoveCommand { x: 1, y: 0 }`。

### 4.2 运行时控制台 (Runtime Console)
游戏视图中的简易输入框（或外部 TCP Socket），接收 JSON 指令。
*   **流程：** `String (JSON)` -> `CommandParser` -> `CommandBus` -> `Logic`。

### 4.3 状态快照 (State Snapshotting)
在 RootState 实现 `ISnapshot` 接口。
*   `GetSnapshot()`: 返回 `GameState` 的完整 JSON。
*   `RestoreSnapshot(json)`: 恢复状态（用于“重试这一回合”式的 Debug）。

## 5. 表现层绑定 (MVVM)

将纯逻辑连接到 Unity 的表现层。

### 5.1 绑定器 (The Binder)
使用 `VContainer` 或简单的 Dictionary 映射。
```csharp
// Game.Presentation/Views/PlayerView.cs
public class PlayerView : MonoBehaviour, IView<PlayerEntity> {
    public Text MeshRenderer;
    
    public void Bind(PlayerEntity entity) {
        // 单向绑定 (Logic -> View)
        entity.HP.Subscribe(hp => UpdateHealthBar(hp)).AddTo(this);
        
        // 插值支持
        // 从 entity 读取 'PreviousPos' 和 'CurrentPos'，并使用 TickEngine.Alpha 进行 Lerp
    }
}
```

## 6. 目录结构 (优化版)

```text
Assets/
  _Project/
    Framework/          [Asmdef: Framework] (除 Debug 外不引用 Unity)
      Core/             (TickEngine, FSM, EventBus)
      Data/             (BindableProperty, 序列化)
      
    Game.Domain/        [Asmdef: Game.Domain] (引用: Framework)
      Models/           (GameState, PlayerState - POCOs)
      Commands/         (MoveCommand, AttackCommand)
      Systems/          (MovementSystem, DamageContext)
      
    Game.Presentation/  [Asmdef: Game.Presentation] (引用: Game.Domain, Framework, Unity)
      Installers/       (VContainer 配置)
      Views/            (PlayerView, HUD)
      Inputs/           (UnityInput -> CommandFactory)
      
    Game.Infrastructure/[Asmdef: Game.Infra] (引用: Game.Domain, Unity)
      Persistence/      (文件存储)
      Network/
```

## 7. 推荐 AI 工作流

1.  **定义：** 用户创建 `Design/Combat.md`。
2.  **Schema：** 用户要求 AI “根据 `Combat.md` 在 `Game.Domain` 中实现 `AttackCommand` 并更新 `GameState`”。
3.  **测试：** AI 编写 `CombatTests.cs` (Headless NUnit)。
4.  **验证：** 用户运行测试（极快）。
5.  **绑定：** 用户要求 AI “创建 `EnemyView.cs` 并绑定到 `EnemyState`”。
