using System;

namespace ALF.Framework.Core
{
    /// <summary>
    /// 提供固定步长的确定性逻辑驱动器。
    /// </summary>
    public sealed class TickEngine
    {
        private readonly float _fixedDeltaTime;
        private double _accumulator;
        private long _tickCount;

        /// <summary>
        /// 初始化 <see cref="TickEngine"/>。
        /// </summary>
        /// <param name="fixedDeltaTime">固定逻辑步长（秒）。</param>
        public TickEngine(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(fixedDeltaTime));
            }

            _fixedDeltaTime = fixedDeltaTime;
        }

        /// <summary>
        /// 当逻辑帧推进时触发。
        /// </summary>
        public event Action<TickContext> OnTick;

        /// <summary>
        /// 获取累计逻辑帧数量。
        /// </summary>
        public long TickCount => _tickCount;

        /// <summary>
        /// 获取固定逻辑步长（秒）。
        /// </summary>
        public float FixedDeltaTime => _fixedDeltaTime;

        /// <summary>
        /// 获取当前帧可用于表现层插值的 alpha。
        /// </summary>
        public float InterpolationAlpha => (float)(_accumulator / _fixedDeltaTime);

        /// <summary>
        /// 输入一帧真实时间并推进到一个或多个逻辑帧。
        /// </summary>
        /// <param name="unityDeltaTime">外部提供的帧间隔时间（秒）。</param>
        public void Update(float unityDeltaTime)
        {
            if (unityDeltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(unityDeltaTime));
            }

            _accumulator += unityDeltaTime;
            while (_accumulator >= _fixedDeltaTime)
            {
                _tickCount += 1;
                OnTick?.Invoke(new TickContext(_fixedDeltaTime, _tickCount));
                _accumulator -= _fixedDeltaTime;
            }
        }

        /// <summary>
        /// 重置内部累加器与逻辑帧计数。
        /// </summary>
        public void Reset()
        {
            _accumulator = 0d;
            _tickCount = 0;
        }
    }
}
