using ALF.Game.Domain.Models;

namespace ALF.Game.Domain.Infrastructure
{
    /// <summary>
    /// 定义游戏状态存取契约。
    /// </summary>
    public interface IGameStateStore
    {
        /// <summary>
        /// 加载当前游戏状态。
        /// </summary>
        /// <returns>当前游戏状态实例。</returns>
        GameState Load();

        /// <summary>
        /// 保存当前游戏状态。
        /// </summary>
        /// <param name="state">待保存状态。</param>
        void Save(GameState state);
    }
}
