using ALF.Framework.Data;

namespace ALF.Game.Domain.Models
{
    /// <summary>
    /// 表示游戏根状态。
    /// </summary>
    public sealed class GameState
    {
        /// <summary>
        /// 初始化 <see cref="GameState"/>。
        /// </summary>
        public GameState()
        {
            GlobalTime = new BindableProperty<float>(0f);
            Player = new PlayerState();
        }

        /// <summary>
        /// 获取全局时间。
        /// </summary>
        public BindableProperty<float> GlobalTime { get; }

        /// <summary>
        /// 获取玩家状态。
        /// </summary>
        public PlayerState Player { get; }

        /// <summary>
        /// 计算当前状态的确定性摘要哈希。
        /// </summary>
        /// <returns>用于回放一致性比较的哈希值。</returns>
        public int ComputeDeterministicHash()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + GlobalTime.Value.GetHashCode();
                hash = (hash * 31) + Player.Health.Value.GetHashCode();
                hash = (hash * 31) + Player.PositionX.Value.GetHashCode();
                hash = (hash * 31) + Player.PositionY.Value.GetHashCode();
                return hash;
            }
        }
    }
}
