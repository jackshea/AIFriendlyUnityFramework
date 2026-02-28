# Worktree Policy

## 1. 目的

为 AI 协作降低上下文污染，每个 worktree 只处理一条能力链。

## 2. 约束

- 一个 worktree 只做一个主题（例如 `feature/tick-core`）。
- 不在同一 worktree 混合 Domain 与大量美术改动。
- 每个 worktree 都必须可独立通过基础检查脚本。

## 3. 建议流程

1. 从主分支创建新 worktree。
2. 先补测试，再补实现。
3. 运行 `./scripts/verify-domain-purity.sh`。
4. 再提交 PR。

## 4. AI 使用建议

- 启动会话前先读取 `ARCHITECTURE.md`。
- 实现前先读取 `.ai/templates/*` 模板。
- 如需跨模块修改，拆为多个 PR，保持可回滚性。
