using ALF.Framework.Core;
using UnityEngine;

namespace ALF.Game.Presentation.Views
{
    /// <summary>
    /// 将 Unity 帧时间输入到确定性逻辑引擎。
    /// </summary>
    public sealed class GameLoopDriver : MonoBehaviour
    {
        [SerializeField]
        private float fixedDeltaTime = 0.02f;

        private TickEngine tickEngine;

        /// <summary>
        /// 获取当前逻辑引擎实例。
        /// </summary>
        public TickEngine TickEngine => tickEngine;

        /// <summary>
        /// 初始化逻辑引擎。
        /// </summary>
        private void Awake()
        {
            tickEngine = new TickEngine(fixedDeltaTime);
        }

        /// <summary>
        /// 每帧推进逻辑引擎。
        /// </summary>
        private void Update()
        {
            tickEngine.Update(Time.deltaTime);
        }
    }
}
