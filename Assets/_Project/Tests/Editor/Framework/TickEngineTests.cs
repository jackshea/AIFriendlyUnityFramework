using System.Collections.Generic;
using ALF.Framework.Core;
using NUnit.Framework;

namespace ALF.Tests.Editor.Framework
{
    /// <summary>
    /// 验证 TickEngine 的固定步长行为。
    /// </summary>
    public sealed class TickEngineTests
    {
        /// <summary>
        /// 累积时间达到步长时应触发对应次数的逻辑帧。
        /// </summary>
        [Test]
        public void Update_WhenAccumulatedTimeExceedsStep_ShouldEmitExpectedTicks()
        {
            TickEngine engine = new TickEngine(0.02f);
            List<long> ticks = new List<long>();
            engine.OnTick += context => ticks.Add(context.TickIndex);

            engine.Update(0.05f);

            Assert.AreEqual(2, ticks.Count);
            Assert.AreEqual(1, ticks[0]);
            Assert.AreEqual(2, ticks[1]);
            Assert.AreEqual(2, engine.TickCount);
        }
    }
}
