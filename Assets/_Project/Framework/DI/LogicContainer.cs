using VContainer;

namespace ALF.Framework.DI
{
    /// <summary>
    /// 对象解析容器的框架级封装。
    /// </summary>
    public sealed class LogicContainer
    {
        private readonly IObjectResolver _resolver;

        /// <summary>
        /// 初始化 <see cref="LogicContainer"/>。
        /// </summary>
        /// <param name="resolver">底层对象解析器。</param>
        public LogicContainer(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// 解析指定类型实例。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <returns>解析出的实例。</returns>
        public T Resolve<T>()
        {
            return _resolver.Resolve<T>();
        }
    }
}
