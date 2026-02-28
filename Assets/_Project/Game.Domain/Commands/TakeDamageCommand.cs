using ALF.Framework.Core;

namespace ALF.Game.Domain.Commands
{
    /// <summary>
    /// 表示玩家受伤命令。
    /// </summary>
    public sealed class TakeDamageCommand : ICommand
    {
        /// <summary>
        /// 初始化 <see cref="TakeDamageCommand"/>。
        /// </summary>
        /// <param name="amount">伤害值。</param>
        public TakeDamageCommand(float amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// 获取伤害值。
        /// </summary>
        public float Amount { get; }
    }
}
