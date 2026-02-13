# 关于MVVM生命周期

设计一个清晰的 MVVM 生命周期管理系统，对于 AI 辅助开发至关重要。如果生命周期混乱，AI 生成的代码极易引发内存泄漏（忘记解绑事件）或空引用异常（View 被销毁但 ViewModel 还在运作）。

在 AI 友好的架构中，我们通过引入一个核心的 **UIManager（UI 编排器）** 来统一管理这三者的生老病死。

以下是 MVVM 生命周期的详细处理方案及核心代码设计。

------

### 1. 职责划分与持有关系 (Ownership & Retention)

明确“谁创建谁、谁持有谁”，这是 AI 编写无 Bug 代码的基石。

- **Model (数据/服务层)**：
  - **创建与持有**：由全局 DI 容器（如 VContainer）在游戏启动时创建并持有。
  - **生命周期**：通常与游戏进程一致（单例服务）或与特定场景绑定。
- **ViewModel (逻辑控制层)**：
  - **创建**：由 `UIManager` 动态实例化（通常通过 DI 容器的 Factory 生成，以便注入 Model）。
  - **持有**：被 `UIManager` 持有（用于路由和栈管理），同时被 `View` 临时引用。
  - **销毁**：UI 关闭时，`UIManager` 移除对其的引用，C# 垃圾回收器（GC）自动回收。
- **View (表现层/预制体)**：
  - **创建**：由 `UIManager` 通过资源加载系统（如 Addressables）实例化 Unity Prefab。
  - **持有**：挂载在 Unity Scene 的 Canvas 节点下。
  - **销毁**：UI 关闭时，调用 `GameObject.Destroy()`，并必须在 `OnDestroy` 中彻底注销 ViewModel 的事件订阅。

------

### 2. 生命周期四步曲 (The 4-Step Lifecycle)

#### 步骤一：请求与实例化 (Creation)

当业务逻辑需要打开一个界面时，不直接操作 Prefab，而是向 `UIManager` 发起请求。

`UIManager` 负责异步加载对应的 UI 预制体，并将其作为 GameObject 实例化到 Canvas 下。

#### 步骤二：装配与绑定 (Assembly & Binding)

预制体实例化后，`UIManager` 获取其挂载的 `View` 脚本。接着，`UIManager` 利用 DI 容器创建对应的 `ViewModel`，并将依赖的 `Model` 注入其中。最后，调用 `View.Bind(ViewModel)` 完成两者连接。

#### 步骤三：运行与交互 (Active State)

此时 UI 处于激活状态。玩家点击按钮，触发 `View` 调用的 `ViewModel.OnButtonClick()`；数据变化时，`ViewModel` 的 `BindableProperty` 通知 `View` 更新画面。

#### 步骤四：解绑与销毁 (Destruction)

关闭界面时，调用 `UIManager.CloseWindow()`。

**重点机制**：必须在 `View` 的 `OnDestroy` 方法中，显式调用 `ViewModel` 属性的 `Unsubscribe`。解绑后，销毁 GameObject，`ViewModel` 失去所有强引用，随后被 GC 安全回收。

------

### 3. 核心代码实现模板 (AI-Friendly Code)

为了让 Claude Code 能够无脑生成 UI，我们需要定义一套高度标准化的基类和接口。

#### A. 标准化接口

AI 需要明确的接口契约来进行跨文件生成。

C#

```
// 所有 ViewModel 必须实现的空接口，用于泛型约束
public interface IViewModel { }

// 所有 View 必须实现的接口
public interface IView<TViewModel> where TViewModel : IViewModel
{
    void Bind(TViewModel viewModel);
}
```

#### B. UIManager 编排器 (核心调度中心)

这个类封装了所有底层的“脏活”，对外只暴露极为简洁的 API。我们假定使用 Addressables 进行资源加载，VContainer 进行依赖注入。

C#

```
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

public class UIManager
{
    private readonly IObjectResolver _resolver; // 依赖注入容器
    private readonly Transform _uiRoot;         // Canvas 根节点

    public UIManager(IObjectResolver resolver, Transform uiRoot)
    {
        _resolver = resolver;
        _uiRoot = uiRoot;
    }

    // AI 调用此方法即可一键完成：加载Prefab -> 实例化 -> 创建VM -> 注入数据 -> 绑定
    public async void OpenWindow<TView, TViewModel>(string addressableKey) 
        where TView : MonoBehaviour, IView<TViewModel>
        where TViewModel : IViewModel
    {
        // 1. 实例化 View (Prefab)
        var handle = Addressables.InstantiateAsync(addressableKey, _uiRoot);
        GameObject viewGo = await handle.Task;
        
        TView viewComponent = viewGo.GetComponent<TView>();

        // 2. 实例化 ViewModel 并自动注入其需要的 Model/Services
        TViewModel viewModel = _resolver.Instantiate<TViewModel>();

        // 3. 绑定 View 与 ViewModel
        viewComponent.Bind(viewModel);
    }

    public void CloseWindow(GameObject viewGo)
    {
        // 1. 销毁 GameObject，这会触发 MonoBehaviour 的 OnDestroy()
        Object.Destroy(viewGo);
        
        // 注意：无需手动销毁 ViewModel。
        // 只要 View 在 OnDestroy 中正确解绑了事件，ViewModel 将没有引用，会被 C# GC 自动回收。
    }
}
```

#### C. View 基类模板 (防内存泄漏设计)

为了防止 AI 或人类忘记解绑事件导致内存泄漏，可以提供一个带有自动清理机制的抽象基类。

C#

```
using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseView<TViewModel> : MonoBehaviour, IView<TViewModel> where TViewModel : IViewModel
{
    protected TViewModel ViewModel { get; private set; }
    
    // 收集所有订阅事件，便于在销毁时统一注销
    private readonly List<Action> _unbindActions = new List<Action>();

    public virtual void Bind(TViewModel viewModel)
    {
        ViewModel = viewModel;
        OnBind(); // 让子类去实现具体的绑定逻辑
    }

    protected abstract void OnBind();

    // 辅助方法：AI 生成代码时使用此方法订阅，框架会自动管理生命周期
    protected void Subscribe<T>(BindableProperty<T> property, Action<T> callback)
    {
        property.Subscribe(callback);
        _unbindActions.Add(() => property.Unsubscribe(callback));
    }

    private void OnDestroy()
    {
        // 生命周期终点：彻底解绑所有事件，切断 View 与 ViewModel 的联系
        foreach (var unbindAction in _unbindActions)
        {
            unbindAction?.Invoke();
        }
        _unbindActions.Clear();
        ViewModel = default;
    }
}
```

------

### 4. AI 视角下的开发体验

有了上述框架，当你想让 Claude Code 添加一个“设置面板”时，指令和执行过程会非常线性：

1. **你的指令**：“创建一个 SettingsViewModel，包含音量大小的 BindableProperty，并依赖 IAudioModel。然后创建一个 SettingsView，继承 BaseView，绑定音量滑动条。”
2. **AI 生成 VM**：通过构造函数请求 `IAudioModel`，不涉及任何 Unity API。
3. **AI 生成 View**：使用 `Subscribe(ViewModel.Volume, OnVolumeChanged)`，无需关心解绑和销毁逻辑，基类已代劳。
4. **业务调用**：AI 在主菜单逻辑中写下 `UIManager.OpenWindow<SettingsView, SettingsViewModel>("UI_Settings");`，整个流程跑通。

接下来，我们需要处理 UI 的层级管理（例如弹窗覆盖、返回键回退逻辑）。你想让我为你设计一套基于栈（Stack）的 UI 路由管理器，还是先深入探讨如何用代码自动挂载并映射 UI 预制体中的组件（避免在 Inspector 中手动拖拽）？