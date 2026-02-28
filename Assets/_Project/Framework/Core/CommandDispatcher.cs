using System;
using System.Collections.Generic;

namespace ALF.Framework.Core
{
    /// <summary>
    /// 提供命令到处理器的显式分发。
    /// </summary>
    public sealed class CommandDispatcher
    {
        private readonly Dictionary<Type, Action<ICommand, TickContext>> _handlers = new Dictionary<Type, Action<ICommand, TickContext>>();

        /// <summary>
        /// 注册指定命令类型的处理器。
        /// </summary>
        /// <typeparam name="TCommand">命令类型。</typeparam>
        /// <param name="handler">命令处理器实例。</param>
        public void Register<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers[typeof(TCommand)] = (command, context) => handler.Handle((TCommand)command, context);
        }

        /// <summary>
        /// 分发命令到匹配处理器。
        /// </summary>
        /// <param name="command">待处理命令。</param>
        /// <param name="context">当前逻辑帧上下文。</param>
        public void Dispatch(ICommand command, TickContext context)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Type commandType = command.GetType();
            if (!_handlers.TryGetValue(commandType, out Action<ICommand, TickContext> forward))
            {
                throw new InvalidOperationException("No handler registered for command type: " + commandType.FullName);
            }

            forward(command, context);
        }
    }
}
