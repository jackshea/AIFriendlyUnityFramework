using ALF.Framework.Core;
using ALF.Game.Domain.Commands;
using ALF.Game.Domain.Models;
using ALF.Game.Domain.Systems;
using NUnit.Framework;

namespace ALF.Tests.Editor.Domain
{
    /// <summary>
    /// 验证同输入序列下状态回放一致性。
    /// </summary>
    public sealed class DeterministicReplayTests
    {
        /// <summary>
        /// 相同命令序列在相同步长下应产生相同状态哈希。
        /// </summary>
        [Test]
        public void Replay_WithSameCommands_ShouldProduceSameStateHash()
        {
            int hash1 = RunScenario();
            int hash2 = RunScenario();

            Assert.AreEqual(hash1, hash2);
        }

        /// <summary>
        /// 运行一段固定命令序列并返回最终状态哈希。
        /// </summary>
        /// <returns>最终状态哈希。</returns>
        private static int RunScenario()
        {
            GameState state = new GameState();
            TimeAdvanceSystem timeSystem = new TimeAdvanceSystem(state);
            MoveCommandHandler moveHandler = new MoveCommandHandler(state.Player);
            TakeDamageCommandHandler damageHandler = new TakeDamageCommandHandler(state.Player);

            CommandDispatcher dispatcher = new CommandDispatcher();
            dispatcher.Register(moveHandler);
            dispatcher.Register(damageHandler);

            TickEngine engine = new TickEngine(0.02f);
            engine.OnTick += context => timeSystem.OnTick(context);

            TickContext tick1 = new TickContext(0.02f, 1);
            TickContext tick2 = new TickContext(0.02f, 2);
            TickContext tick3 = new TickContext(0.02f, 3);

            dispatcher.Dispatch(new MoveCommand(1.5f, -0.5f), tick1);
            dispatcher.Dispatch(new TakeDamageCommand(3f), tick2);
            dispatcher.Dispatch(new MoveCommand(-0.5f, 0.5f), tick3);

            engine.Update(0.06f);

            return state.ComputeDeterministicHash();
        }
    }
}
