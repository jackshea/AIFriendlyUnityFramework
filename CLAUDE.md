# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是 **ALF (AI-Native Logic-First Framework)** - 一个专为 AI 辅助开发优化的 Unity 框架。核心目标是创建"透明盒"架构，让 AI（如 Claude Code）能够理解、修改和调试游戏逻辑，而不被 Unity 的复杂性所困扰。

## 核心架构原则

### 1. 严格的层分离（通过 .asmdef 强制）

```
Assets/_Project/
├── Framework/              [Asmdef: Framework]
│   ├── Core/               (TickEngine, EventBus, Base Classes)
│   ├── Data/               (BindableProperty, Serialization)
│   ├── AIBridge/           (State Export, Command Registry)
│   └── DI/                 (VContainer extensions)
│
├── Game.Domain/            [Asmdef: Game.Domain] ⚠️ 禁止引用 UnityEngine
│   ├── Models/             (POCO data classes)
│   ├── Commands/           (MoveCommand, AttackCommand, etc.)
│   ├── Systems/            (Game logic systems)
│   └── Infrastructure/     (Storage, Network interfaces)
│
├── Game.Presentation/      [Asmdef: Game.Presentation]
│   ├── Views/              (MonoBehaviours, UI, VFX)
│   ├── ViewModels/         (Data binding layer)
│   ├── Installers/         (VContainer configuration)
│   └── Inputs/             (Unity input to commands)
│
└── Tests/
    ├── Editor/             (Headless unit tests for Domain)
    └── Runtime/            (Unity integration tests)
```

### 2. Domain 层规则（绝对不可违反）

- **禁止** 使用 `UnityEngine` 命名空间下的任何类型
- **禁止** 使用 `MonoBehaviour`、`Transform`、`GameObject`
- **禁止** 使用 `Time.deltaTime`，必须使用 `TickContext` 获取时间
- **禁止** Singleton 模式，所有服务必须通过 VContainer 注入
- 状态必须存储在 `BindableProperty<T>` 或 `ReactiveCollection<T>` 中
- 所有公共接口必须有 XML 文档注释

### 3. 确定性逻辑系统

游戏逻辑通过 `TickEngine` 以固定时间步长运行：

```csharp
// 逻辑层使用 TickContext
public void OnTick(TickContext ctx) {
    player.Position += velocity * ctx.DeltaTime;
}
```

### 4. 命令模式

所有游戏操作必须通过 Command 对象：
- 逻辑层只处理 Command，不直接调用方法
- Command 可序列化，支持回放和调试
- 格式：`ICommand` 接口，具体实现如 `MoveCommand`、`AttackCommand`

### 5. 数据驱动

- **Config Data**: 只读配置（JSON/Excel），如技能数值、怪物属性
- **Runtime State**: 运行时状态，存储在 `GameState` POCO 类中
- ScriptableObject 只作为编辑工具，进入逻辑层前必须序列化为 POCO

## 依赖关系

- **VContainer**: 依赖注入（替代 Singleton）
- **UniRx** 或自定义实现: 响应式属性
- **NUnit**: 单元测试
- **Addressables**: 资源管理（计划）

## AI 开发工作流

1. **定义**: 在 `Game.Domain/Models` 中定义数据模型
2. **测试**: 在 `Tests/Editor/` 中先写 NUnit 测试（Headless）
3. **实现**: 在 `Game.Domain/Systems` 中实现逻辑
4. **验证**: 运行测试（毫秒级反馈）
5. **绑定**: 在 `Game.Presentation` 中实现 View 层

## 代码规范

- 每个类最大 150 行（AI 上下文优化）
- Feature-based 目录结构（按功能垂直切分）
- 契约式数据：使用 `[Invariant]` 属性定义数据约束
- 事件溯源：所有状态变更通过 Command 记录，支持回放

## 文档位置

详细设计文档位于 `Docs/V1/` 目录：
- `ALF 2.0 深度设计方案：透明盒架构.md` - 核心架构设计
- `技术设计规范 (Framework Design).md` - 技术实现细节
- `目录划分.md` - 程序集和目录结构
- `数据驱动的设计.md` - 数据驱动模式
- `AI上下文提示词 (AI Context Prompt).md` - AI 协作模板
