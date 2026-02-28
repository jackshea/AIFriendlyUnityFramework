using ALF.Framework.Core;
using ALF.Game.Domain.Commands;
using ALF.Game.Domain.Models;

namespace ALF.Game.Domain.Systems
{
    /// <summary>
    /// 处理玩家移动命令。
    /// </summary>
    public sealed class MoveCommandHandler : ICommandHandler<MoveCommand>
    {
        private readonly PlayerState _player;

        /// <summary>
        /// 初始化 <see cref="MoveCommandHandler"/>。
        /// </summary>
        /// <param name="player">玩家状态引用。</param>
        public MoveCommandHandler(PlayerState player)
        {
            _player = player;
        }

        /// <summary>
        /// 应用移动命令。
        /// </summary>
        /// <param name="command">移动命令。</param>
        /// <param name="context">当前逻辑帧上下文。</param>
        public void Handle(MoveCommand command, TickContext context)
        {
            _player.PositionX.Value += command.DeltaX;
            _player.PositionY.Value += command.DeltaY;
        }
    }

    /// <summary>
    /// 处理玩家受伤命令。
    /// </summary>
    public sealed class TakeDamageCommandHandler : ICommandHandler<TakeDamageCommand>
    {
        private readonly PlayerState _player;

        /// <summary>
        /// 初始化 <see cref="TakeDamageCommandHandler"/>。
        /// </summary>
        /// <param name="player">玩家状态引用。</param>
        public TakeDamageCommandHandler(PlayerState player)
        {
            _player = player;
        }

        /// <summary>
        /// 应用受伤命令并夹紧到非负值。
        /// </summary>
        /// <param name="command">受伤命令。</param>
        /// <param name="context">当前逻辑帧上下文。</param>
        public void Handle(TakeDamageCommand command, TickContext context)
        {
            float next = _player.Health.Value - command.Amount;
            _player.Health.Value = next < 0f ? 0f : next;
        }
    }
}
