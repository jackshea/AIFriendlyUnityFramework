using ALF.Framework.Core;
using ALF.Game.Domain.Models;

namespace ALF.Game.Domain.Systems
{
    /// <summary>
    /// 负责推进游戏全局时间。
    /// </summary>
    public sealed class TimeAdvanceSystem
    {
        private readonly GameState _state;

        /// <summary>
        /// 初始化 <see cref="TimeAdvanceSystem"/>。
        /// </summary>
        /// <param name="state">游戏状态引用。</param>
        public TimeAdvanceSystem(GameState state)
        {
            _state = state;
        }

        /// <summary>
        /// 在每个逻辑帧推进全局时间。
        /// </summary>
        /// <param name="context">当前逻辑帧上下文。</param>
        public void OnTick(TickContext context)
        {
            _state.GlobalTime.Value += context.DeltaTime;
        }
    }
}
