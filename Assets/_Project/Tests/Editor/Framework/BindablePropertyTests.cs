using ALF.Framework.Data;
using NUnit.Framework;

namespace ALF.Tests.Editor.Framework
{
    /// <summary>
    /// 验证 BindableProperty 的值变更语义。
    /// </summary>
    public sealed class BindablePropertyTests
    {
        /// <summary>
        /// 新值与旧值不同应触发通知并增加版本号。
        /// </summary>
        [Test]
        public void Value_WhenChanged_ShouldIncrementVersionAndNotify()
        {
            BindableProperty<int> property = new BindableProperty<int>(1);
            int observed = -1;
            property.OnValueChanged += value => observed = value;

            property.Value = 7;

            Assert.AreEqual(7, observed);
            Assert.AreEqual(1, property.Version);
        }

        /// <summary>
        /// 新值与旧值相同不应触发通知或增加版本号。
        /// </summary>
        [Test]
        public void Value_WhenUnchanged_ShouldNotIncrementVersion()
        {
            BindableProperty<int> property = new BindableProperty<int>(3);
            int invokeCount = 0;
            property.OnValueChanged += _ => invokeCount += 1;

            property.Value = 3;

            Assert.AreEqual(0, invokeCount);
            Assert.AreEqual(0, property.Version);
        }
    }
}
