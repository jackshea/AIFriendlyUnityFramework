namespace ALF.Framework.Core
{
    /// <summary>
    /// 定义命令处理器契约。
    /// </summary>
    /// <typeparam name="TCommand">命令类型。</typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// 处理命令并应用到当前逻辑帧。
        /// </summary>
        /// <param name="command">待处理命令。</param>
        /// <param name="context">当前逻辑帧上下文。</param>
        void Handle(TCommand command, TickContext context);
    }
}
