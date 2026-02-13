# AI 驱动的 Unity MVVM 框架设计文档

传统 Unity UI 开发高度依赖 Editor 操作（拖拽引用、绑定 UnityEvent），这正是基于文本的 AI 助手（如 Claude Code）最大的盲区。AI 看不到 Prefab 里的层级，也无法帮你用鼠标拖拽组件。

因此，**AI 友好的 MVVM（Model-View-ViewModel）框架，其核心目标是：将 UI 的状态管理和绑定逻辑全面“代码化、文本化”，并彻底剥离业务逻辑与表现层。**

以下是专为 Claude Code 设计的 Unity MVVM 框架详细设计文档。

------

# 🖥️ AI 驱动的 Unity MVVM 框架设计文档

## 1. 核心设计哲学 (UI-as-Code Principles)

- **零业务逻辑视图 (Humble Object)**：`View`（MonoBehaviour）极度克制，绝对不允许包含任何 `if/else` 判断或游戏逻辑。它只是个“无情的绑定机器”。
- **纯 C# 视图模型 (Pure C# ViewModel)**：`ViewModel` 不能继承 `MonoBehaviour`，不引用任何 `UnityEngine.UI` 组件。这使得 Claude 可以毫无阻碍地对其进行 100% 的自动化单元测试。
- **显式代码绑定 (Explicit Code Binding)**：放弃 Inspector 中的 `UnityEvent` 拖拽绑定。所有的 UI 响应和数据更新，都必须在 `View` 的 C# 脚本中通过手写代码显式订阅。AI 可以精准地阅读和生成这些绑定代码。
- **推荐 UI Toolkit (可选但极佳)**：如果项目允许，强烈建议使用 Unity UI Toolkit。其 `UXML` 和 `USS` 是纯文本格式，Claude Code 可以直接阅读和修改 UI 布局，达成完美的“全文本 UI 开发”。

------

## 2. 框架分层与 AI 职责边界

| **组件**                 | **载体类型**               | **AI 的职责与能力边界**                                      |
| ------------------------ | -------------------------- | ------------------------------------------------------------ |
| **Model (模型)**         | 纯 C# POCO / 数据实体      | AI 基于上一篇的“数据驱动”生成，负责提供原始数值。            |
| **ViewModel (视图模型)** | 纯 C# 类                   | **AI 的绝对主场**。包含 `BindableProperty<T>` 和交互命令。AI 负责编写转换逻辑并生成单元测试。 |
| **View (视图)**          | MonoBehaviour / UI Toolkit | AI 负责编写 `Bind()` 方法。人类开发者负责在 Unity 中把 UI 组件拖给脚本的 `public` 字段。 |

------

## 3. 核心基建：轻量级响应式属性 (The Binder)

为了不引入庞大的第三方库（如 UniRx）增加 AI 的认知负担，我们需要为 AI 提供一个极简、语义清晰的响应式属性基类。这是 MVVM 的心脏。

C#

```
// 核心基建：告诉 Claude Code 使用这个类来包装 ViewModel 中的状态
public class BindableProperty<T>
{
    private T _value;
    private Action<T> _onValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            _onValueChanged?.Invoke(_value);
        }
    }

    public void Subscribe(Action<T> callback)
    {
        _onValueChanged += callback;
        callback?.Invoke(_value); // 订阅时立即触发一次初始赋值
    }

    public void Unsubscribe(Action<T> callback)
    {
        _onValueChanged -= callback;
    }
}
```

------

## 4. 给 AI 的开发规范 (`AI_UI_RULES.md`)

在项目中提供这份指南，让 Claude Code 每次写 UI 都遵循规范：

> **UI 开发与 MVVM 规则：**
>
> 1. **View 规范**：View 脚本只能引用 `UnityEngine.UI` 或 `TMPro`。必须包含一个 `public void Bind(ViewModel vm)` 方法。
> 2. **ViewModel 规范**：禁止引入 `UnityEngine`。所有需要绑定到 UI 的状态必须使用 `BindableProperty<T>`。所有的按钮点击事件暴露为 `public void OnXXXClicked()` 方法。
> 3. **双向解耦**：ViewModel 不知道 View 的存在。View 只能读取 ViewModel 的属性并调用其方法，不能修改 ViewModel 的 `BindableProperty.Value`（除非是输入框的双向绑定）。
> 4. **测试优先**：在编写 View 之前，必须先为 ViewModel 编写 NUnit 测试，验证状态转换逻辑。

------

## 5. 典型应用示例：玩家生命值面板

以下是人类与 Claude Code 配合生成一个生命值面板的标准产出：

### 第一步：AI 编写 ViewModel (纯逻辑)

Claude Code 生成的这段代码完美解耦，可独立运行在任何 C# 环境中进行测试。

C#

```
public class PlayerHealthViewModel
{
    // 供 View 订阅的数据
    public BindableProperty<float> HealthPercent { get; } = new BindableProperty<float>();
    public BindableProperty<string> HealthText { get; } = new BindableProperty<string>();

    private int _currentHp;
    private int _maxHp;

    public PlayerHealthViewModel(int initialHp, int maxHp)
    {
        _maxHp = maxHp;
        SetHealth(initialHp);
    }

    // 业务逻辑引发的数据变动
    public void TakeDamage(int damage)
    {
        int newHp = Math.Max(0, _currentHp - damage);
        SetHealth(newHp);
    }

    public void Heal() // 暴露给 View 的 UI 交互命令
    {
        SetHealth(_maxHp);
    }

    private void SetHealth(int hp)
    {
        _currentHp = hp;
        HealthPercent.Value = (float)_currentHp / _maxHp;
        HealthText.Value = $"{_currentHp} / {_maxHp}";
    }
}
```

### 第二步：AI 编写 View (显式绑定)

AI 清楚地知道需要暴露哪些组件，并写好了订阅逻辑。

C#

```
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthView : MonoBehaviour
{
    [Header("UI References")] // 人类只需在 Inspector 拖拽这三个组件
    public Slider HealthSlider;
    public TextMeshProUGUI HealthText;
    public Button HealButton;

    private PlayerHealthViewModel _viewModel;

    // AI 编写的显式绑定逻辑
    public void Bind(PlayerHealthViewModel viewModel)
    {
        _viewModel = viewModel;

        // 绑定数据到 UI
        _viewModel.HealthPercent.Subscribe(UpdateSlider);
        _viewModel.HealthText.Subscribe(UpdateText);

        // 绑定 UI 交互到 ViewModel 命令
        HealButton.onClick.AddListener(_viewModel.Heal);
    }

    private void UpdateSlider(float percent) => HealthSlider.value = percent;
    private void UpdateText(string text) => HealthText.text = text;

    private void OnDestroy()
    {
        // AI 会自动处理解绑，防止内存泄漏
        if (_viewModel != null)
        {
            _viewModel.HealthPercent.Unsubscribe(UpdateSlider);
            _viewModel.HealthText.Unsubscribe(UpdateText);
        }
    }
}
```

------

## 6. AI 驱动 UI 开发的最佳工作流

1. **定义需求 (Human)**：你告诉 Claude：“我需要一个商店界面，显示玩家金币，并有购买生命药水的按钮，药水价格在 `ItemConfig.json` 里。”
2. **生成数据模型与接口 (AI)**：Claude 根据 JSON 生成数据结构，并搭出 `IShopService` 的空壳。
3. **编写 ViewModel 与测试 (AI)**：Claude 编写 `ShopViewModel.cs`，并写好测试（例如：金币不足时，购买命令不触发库存变动）。
4. **生成 View 脚本 (AI)**：Claude 产出 `ShopView.cs`，暴露好 `public` 的 UI 组件字段和 `Bind()` 方法。
5. **组装 (Human/AI)**：
   - *对于 UGUI*：你在 Unity 中创建 Canvas，挂载 `ShopView.cs`，拖拽 Button 和 Text。在某个入口点注入依赖并调用 `view.Bind(new ShopViewModel(...))`。
   - *对于 UI Toolkit*：你可以让 Claude 直接生成配套的 `ShopView.uxml` 文件，实现真正的全流程自动化。

这套设计将 UI 拆分成了“思考的大脑（ViewModel，AI负责）”和“展示的躯壳（View，你/UXML负责）”，最大限度地避免了 AI 处理不了复杂 Prefab 引用的问题。

**下一步，您是想探讨如何让 Claude Code 自动生成基于 UI Toolkit 的 UXML 文件，还是想看一个完整的 ViewModel 单元测试用例是如何编写的？**