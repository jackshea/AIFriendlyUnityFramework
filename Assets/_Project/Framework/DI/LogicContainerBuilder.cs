using VContainer;

namespace ALF.Framework.DI
{
    /// <summary>
    /// 提供面向逻辑层的容器注册入口。
    /// </summary>
    public sealed class LogicContainerBuilder
    {
        private readonly ContainerBuilder _builder = new ContainerBuilder();

        /// <summary>
        /// 注册单例服务类型。
        /// </summary>
        /// <typeparam name="T">服务类型。</typeparam>
        /// <returns>当前构建器。</returns>
        public LogicContainerBuilder RegisterSingleton<T>() where T : class
        {
            _builder.Register<T>(Lifetime.Singleton);
            return this;
        }

        /// <summary>
        /// 注册单例服务映射。
        /// </summary>
        /// <typeparam name="TService">服务抽象类型。</typeparam>
        /// <typeparam name="TImplementation">服务实现类型。</typeparam>
        /// <returns>当前构建器。</returns>
        public LogicContainerBuilder RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _builder.Register<TImplementation>(Lifetime.Singleton).As<TService>();
            return this;
        }

        /// <summary>
        /// 注册已有实例。
        /// </summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="instance">实例对象。</param>
        /// <returns>当前构建器。</returns>
        public LogicContainerBuilder RegisterInstance<T>(T instance) where T : class
        {
            _builder.RegisterInstance(instance);
            return this;
        }

        /// <summary>
        /// 构建不可变逻辑容器。
        /// </summary>
        /// <returns>可用于解析对象的逻辑容器。</returns>
        public LogicContainer Build()
        {
            return new LogicContainer(_builder.Build());
        }
    }
}
