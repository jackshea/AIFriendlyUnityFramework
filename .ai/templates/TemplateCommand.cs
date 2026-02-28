using ALF.Framework.Core;

namespace ALF.Game.Domain.Commands
{
    /// <summary>
    /// 命令模板：描述一次可序列化的领域意图。
    /// </summary>
    public sealed class TemplateCommand : ICommand
    {
        /// <summary>
        /// 初始化 <see cref="TemplateCommand"/>。
        /// </summary>
        /// <param name="value">示例参数。</param>
        public TemplateCommand(int value)
        {
            Value = value;
        }

        /// <summary>
        /// 获取示例参数。
        /// </summary>
        public int Value { get; }
    }
}
