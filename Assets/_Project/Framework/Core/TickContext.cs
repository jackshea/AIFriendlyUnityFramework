namespace ALF.Framework.Core
{
    /// <summary>
    /// 表示一次逻辑帧执行时的只读上下文。
    /// </summary>
    public readonly struct TickContext
    {
        /// <summary>
        /// 初始化 <see cref="TickContext"/>。
        /// </summary>
        /// <param name="deltaTime">固定步长时间（秒）。</param>
        /// <param name="tickIndex">当前逻辑帧序号。</param>
        public TickContext(float deltaTime, long tickIndex)
        {
            DeltaTime = deltaTime;
            TickIndex = tickIndex;
        }

        /// <summary>
        /// 获取固定步长时间（秒）。
        /// </summary>
        public float DeltaTime { get; }

        /// <summary>
        /// 获取当前逻辑帧序号。
        /// </summary>
        public long TickIndex { get; }
    }
}
