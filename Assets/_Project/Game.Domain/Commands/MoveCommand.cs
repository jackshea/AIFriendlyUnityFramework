using ALF.Framework.Core;

namespace ALF.Game.Domain.Commands
{
    /// <summary>
    /// 表示玩家移动命令。
    /// </summary>
    public sealed class MoveCommand : ICommand
    {
        /// <summary>
        /// 初始化 <see cref="MoveCommand"/>。
        /// </summary>
        /// <param name="deltaX">X 轴位移增量。</param>
        /// <param name="deltaY">Y 轴位移增量。</param>
        public MoveCommand(float deltaX, float deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        /// <summary>
        /// 获取 X 轴位移增量。
        /// </summary>
        public float DeltaX { get; }

        /// <summary>
        /// 获取 Y 轴位移增量。
        /// </summary>
        public float DeltaY { get; }
    }
}
