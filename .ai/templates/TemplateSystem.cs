using ALF.Framework.Core;

namespace ALF.Game.Domain.Systems
{
    /// <summary>
    /// 系统模板：实现单一职责的按帧逻辑。
    /// </summary>
    public sealed class TemplateSystem
    {
        /// <summary>
        /// 在逻辑帧中执行系统行为。
        /// </summary>
        /// <param name="context">当前逻辑帧上下文。</param>
        public void OnTick(TickContext context)
        {
        }
    }
}
