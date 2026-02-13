# AI 驱动的 Unity 数据驱动框架设计文档

## 1. 核心设计哲学 (Data-First Principles)

- **文本优先 (Text over Inspector)**：彻底摒弃在 Inspector 中手动配置复杂数据的做法。所有游戏核心数据（如武器属性、关卡刷怪、角色数值）必须以 `JSON`、`YAML` 或 `CSV` 格式存储。
- **契约先行 (Schema-Driven)**：为所有数据文件提供 JSON Schema 或 TypeScript 接口定义。AI 只有在明确知道数据结构（有哪些字段、枚举范围）的情况下，才不会产生“字段幻觉”。
- **代码生成 (Code Generation)**：AI 修改 JSON 数据后，通过命令行工具自动生成对应的 C# 数据类（POCO），确保数据与逻辑的强类型绑定。
- **热重载 (Hot-Reloading)**：运行时监听数据文件的变化，AI 在终端修改配置后，游戏内实时生效，实现“所改即所见”。

------

## 2. 框架分层架构

为了让数据真正驱动逻辑，同时保持 AI 修改的安全边界，系统分为四层：

### 层级 1: 数据源层 (Data Source Layer)

- **载体**：存放在 `Assets/GameData/` 目录下的纯文本文件（推荐 JSON 或 YAML）。
- **AI 交互**：Claude Code 的主阵地。AI 在这里调整游戏平衡性、设计新关卡。

### 层级 2: 数据绑定层 (Data Binding Layer)

- **载体**：自动生成的纯 C# 类（不继承 MonoBehaviour）。
- **AI 交互**：Claude Code 根据 JSON 文件自动编写解析器和反序列化类。
- **示例**：`WeaponConfig.json` 对应生成 `WeaponConfig.cs`。

### 层级 3: 逻辑执行层 (System Layer)

- **载体**：纯粹的业务逻辑脚本（类似 ECS 模式中的 System）。
- **AI 交互**：AI 编写逻辑时，只能从“数据提供者（Data Provider）”读取只读数据，根据数据执行行为。

### 层级 4: 表现层 (View Layer)

- **载体**：MonoBehaviour, 动画, 粒子特效。
- **功能**：纯粹的“木偶”，根据逻辑层传递的数据进行播放。

------

## 3. 给 AI 的规则说明 (`AI_DATA_RULES.md`)

在项目根目录创建此文件，让 Claude Code 了解你的数据规范：

> **数据驱动规则：**
>
> 1. **配置存储**：所有配置表均放置在 `Assets/GameData/Configs/`。
> 2. **禁止硬编码**：任何游戏内的数值（如 `HP = 100`，`Speed = 5.5`）都不允许写死在 C# 代码中，必须从配置读取。
> 3. **Schema 约束**：在修改或创建 JSON 之前，必须先读取 `Assets/GameData/Schema/` 下的结构定义。
> 4. **只读约束**：运行时读取的配置数据在 C# 中必须是 `readonly` 的，禁止在运行时修改基础配置。

------

## 4. 核心组件设计与 AI 工作流

### A. 结构化数据定义 (AI 生成的 JSON & Schema)

首先，让 AI 或你定义一个 Schema，这样后续 AI 生成武器时就不会出错。

**WeaponSchema.json (AI 参考标准):**

JSON

```
{
  "type": "object",
  "properties": {
    "id": { "type": "string" },
    "damage": { "type": "integer" },
    "damageType": { "enum": ["Physical", "Magic", "Fire"] }
  }
}
```

**Weapons.json (AI 实际修改的数据):**

JSON

```
[
  { "id": "sword_01", "damage": 25, "damageType": "Physical" },
  { "id": "staff_01", "damage": 40, "damageType": "Magic" }
]
```

- **优势**：当你需要添加 50 把新武器时，只需对 Claude Code 说：“根据现有的 Weapons.json，再生成 50 把不同流派的武器平衡数据”，AI 就能完美生成。

### B. 强类型数据绑定 (AI 编写的 C# Model)

AI 会根据 JSON 自动生成对应的数据类，并提供全局访问接口。

C#

```
// AI 生成的只读数据模型
[System.Serializable]
public class WeaponConfig 
{
    public string Id;
    public int Damage;
    public string DamageType;
}

// AI 生成的数据管理器 (结合了之前提到的依赖注入)
public interface IWeaponDataProvider 
{
    WeaponConfig GetWeapon(string id);
}
```

### C. 文件监听与热重载 (Hot-Reload)

为了让 Claude Code 成为你的“实时数值策划”，框架需要提供热更新能力。

C#

```
public class ConfigHotReloader : MonoBehaviour
{
    private FileSystemWatcher _watcher;

    void Start()
    {
#if UNITY_EDITOR
        _watcher = new FileSystemWatcher(Application.dataPath + "/GameData/Configs", "*.json");
        _watcher.Changed += OnConfigChanged;
        _watcher.EnableRaisingEvents = true;
#endif
    }

    private void OnConfigChanged(object sender, FileSystemEventArgs e)
    {
        // 触发前面设计的 EventBus
        EventBus.Publish(new ConfigReloadedEvent { FileName = e.Name });
    }
}
```

------

## 5. 典型协作循环 (The Data-Driven Loop)

假设你要开发一个“敌人生成系统”：

1. **定义数据 (Human/AI)**：在终端让 Claude 创建一个 `LevelSpawns.json`，里面写明第一关会在 10 秒、20 秒分别刷出什么怪物。
2. **生成解析 (AI)**：让 Claude 编写 C# 脚本，将这个 JSON 反序列化为内存中的 `List<SpawnData>`。
3. **编写系统 (AI)**：让 Claude 编写 `EnemySpawnSystem.cs`，该脚本在 `Update` 中读取时间，比对配置数据，时间到了就调用生成指令。
4. **实时微调 (AI/Human)**：游戏运行中，你发现怪刷得太慢。你直接在终端告诉 Claude：“把 `LevelSpawns.json` 里的刷怪间隔全部缩短 30%”。Claude 修改文本后，Unity 的 `FileSystemWatcher` 监听到修改，重载配置，你立刻在游戏里看到了变化。

------

通过这种纯文本、强契约、热重载的设计，你实际上是把 Unity 的核心逻辑抽离到了 Claude Code 最擅长的“文本编辑区”，最大限度地发挥了 LLM 的代码和数据生成能力。

**您是希望我为您提供一个能够自动将 JSON 转换为 C# 类的 Python 脚本示例，还是展示如何在 Unity 中写一个通用的 JSON 配置加载与缓存中心？**