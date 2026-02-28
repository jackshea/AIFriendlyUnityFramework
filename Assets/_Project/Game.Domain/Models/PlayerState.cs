using ALF.Framework.Data;

namespace ALF.Game.Domain.Models
{
    /// <summary>
    /// 表示玩家的运行时状态。
    /// </summary>
    public sealed class PlayerState
    {
        /// <summary>
        /// 初始化 <see cref="PlayerState"/>。
        /// </summary>
        public PlayerState()
        {
            Health = new BindableProperty<float>(100f);
            PositionX = new BindableProperty<float>(0f);
            PositionY = new BindableProperty<float>(0f);
        }

        /// <summary>
        /// 获取玩家生命值。
        /// </summary>
        public BindableProperty<float> Health { get; }

        /// <summary>
        /// 获取玩家 X 轴位置。
        /// </summary>
        public BindableProperty<float> PositionX { get; }

        /// <summary>
        /// 获取玩家 Y 轴位置。
        /// </summary>
        public BindableProperty<float> PositionY { get; }
    }
}
