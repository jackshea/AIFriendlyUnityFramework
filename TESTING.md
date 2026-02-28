# Testing Guide

## 1. 测试层次

- Editor 头测：`Assets/_Project/Tests/Editor`
- Runtime 集成测试：`Assets/_Project/Tests/Runtime`

## 2. v1 必测能力

- `TickEngine` 固定步长推进行为。
- `BindableProperty<T>` 值变化通知与版本语义。
- 回放一致性：同命令序列应输出同状态哈希。

## 3. 本地验证步骤

1. 运行纯度检查：
   - `./scripts/verify-domain-purity.sh`
2. 在 Unity Editor 中运行 EditMode 测试。
3. 确认新增代码不破坏 asmdef 依赖方向。

## 4. CI 门禁

`quality-gates` workflow 包含：

- Domain 纯度检查（必跑）。
- 治理文档存在性检查（必跑）。
- EditMode 测试（当 `UNITY_LICENSE` 配置后自动启用）。
