using ALF.Game.Domain.Infrastructure;
using ALF.Game.Domain.Models;

namespace ALF.Game.Infrastructure.Persistence
{
    /// <summary>
    /// 提供内存态游戏状态存储实现。
    /// </summary>
    public sealed class InMemoryGameStateStore : IGameStateStore
    {
        private GameState state = new GameState();

        /// <summary>
        /// 加载当前游戏状态。
        /// </summary>
        /// <returns>当前游戏状态实例。</returns>
        public GameState Load()
        {
            return state;
        }

        /// <summary>
        /// 保存当前游戏状态。
        /// </summary>
        /// <param name="newState">待保存状态。</param>
        public void Save(GameState newState)
        {
            state = newState;
        }
    }
}
