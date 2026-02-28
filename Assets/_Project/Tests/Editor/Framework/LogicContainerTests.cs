using ALF.Framework.Core;
using ALF.Framework.DI;
using ALF.Game.Domain.Commands;
using ALF.Game.Domain.Models;
using ALF.Game.Domain.Systems;
using NUnit.Framework;

namespace ALF.Tests.Editor.Framework
{
    /// <summary>
    /// 验证逻辑层可通过容器完成纯 C# 依赖注入。
    /// </summary>
    public sealed class LogicContainerTests
    {
        /// <summary>
        /// 使用容器解析命令处理器时应共享同一玩家状态实例。
        /// </summary>
        [Test]
        public void ResolveHandlers_WithSharedPlayerState_ShouldMutateSameState()
        {
            LogicContainerBuilder builder = new LogicContainerBuilder();
            builder.RegisterSingleton<PlayerState>();
            builder.RegisterSingleton<MoveCommandHandler>();
            builder.RegisterSingleton<TakeDamageCommandHandler>();

            LogicContainer container = builder.Build();
            PlayerState player = container.Resolve<PlayerState>();
            MoveCommandHandler moveHandler = container.Resolve<MoveCommandHandler>();
            TakeDamageCommandHandler damageHandler = container.Resolve<TakeDamageCommandHandler>();

            TickContext tick = new TickContext(0.02f, 1);
            moveHandler.Handle(new MoveCommand(2f, -1f), tick);
            damageHandler.Handle(new TakeDamageCommand(5f), tick);

            Assert.AreEqual(2f, player.PositionX.Value);
            Assert.AreEqual(-1f, player.PositionY.Value);
            Assert.AreEqual(95f, player.Health.Value);
        }
    }
}
