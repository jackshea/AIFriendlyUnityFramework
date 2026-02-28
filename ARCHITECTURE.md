# ALF Architecture Guide

## 1. 目标

本仓库采用 AI-Native Logic-First 设计：

- 逻辑层必须可在脱离 Unity 的条件下验证。
- AI 修改代码时必须在清晰边界内工作。
- 核心逻辑必须可复盘与可证明（回放一致性）。

## 2. 分层与程序集

- `Assets/_Project/Framework` (`ALF.Framework`): 纯 C# 基础能力。
- `Assets/_Project/Game.Domain` (`ALF.Game.Domain`): 纯 C# 业务逻辑，不得引用 Unity。
- `Assets/_Project/Game.Presentation` (`ALF.Game.Presentation`): Unity 表现层。
- `Assets/_Project/Game.Infrastructure` (`ALF.Game.Infrastructure`): 外部实现适配。
- `Assets/_Project/Tests/Editor` (`ALF.Tests.Editor`): 逻辑层头测。

## 3. 绝对红线

`Game.Domain` 禁止使用：

- `using UnityEngine`
- `MonoBehaviour`
- `GameObject`
- `Transform`
- `Time.deltaTime`

说明：CI 通过 `scripts/verify-domain-purity.sh` 强制检查。

## 4. 命令与时序

- 所有状态改变通过 `ICommand` + `ICommandHandler<T>` 执行。
- 时间推进以 `TickEngine` + `TickContext` 为准。
- 表现层只消费状态，不直接驱动领域状态。

## 5. AI 协作规则

- 先改 Domain 再改 Presentation。
- 新增公共类和公共方法必须补充中文 XML 注释。
- 每次改动必须至少包含一个可验证点（测试或脚本检查）。
