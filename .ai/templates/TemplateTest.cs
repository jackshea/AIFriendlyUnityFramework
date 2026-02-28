using NUnit.Framework;

namespace ALF.Tests.Editor.Template
{
    /// <summary>
    /// 测试模板：先写失败测试再补实现。
    /// </summary>
    public sealed class TemplateTest
    {
        /// <summary>
        /// 示例断言模板。
        /// </summary>
        [Test]
        public void Example_ShouldPassAfterImplementation()
        {
            Assert.IsTrue(true);
        }
    }
}
