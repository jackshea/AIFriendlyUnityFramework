using System;
using System.Collections.Generic;

namespace ALF.Framework.Data
{
    /// <summary>
    /// 提供可观测且可版本追踪的值包装器。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    public sealed class BindableProperty<T>
    {
        private T _value;

        /// <summary>
        /// 初始化 <see cref="BindableProperty{T}"/>。
        /// </summary>
        /// <param name="initialValue">初始值。</param>
        public BindableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        /// <summary>
        /// 当值变化时触发。
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// 获取当前值。
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                {
                    return;
                }

                _value = value;
                Version += 1;
                OnValueChanged?.Invoke(_value);
            }
        }

        /// <summary>
        /// 获取值变更版本号。
        /// </summary>
        public int Version { get; private set; }
    }
}
