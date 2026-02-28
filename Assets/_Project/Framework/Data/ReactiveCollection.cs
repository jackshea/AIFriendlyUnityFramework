using System;
using System.Collections;
using System.Collections.Generic;

namespace ALF.Framework.Data
{
    /// <summary>
    /// 提供可观测的集合包装器。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public sealed class ReactiveCollection<T> : IEnumerable<T>
    {
        private readonly List<T> _items = new List<T>();

        /// <summary>
        /// 当元素被添加时触发。
        /// </summary>
        public event Action<T> OnItemAdded;

        /// <summary>
        /// 当元素被移除时触发。
        /// </summary>
        public event Action<T> OnItemRemoved;

        /// <summary>
        /// 获取元素数量。
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 获取指定索引元素。
        /// </summary>
        /// <param name="index">元素索引。</param>
        public T this[int index] => _items[index];

        /// <summary>
        /// 添加元素。
        /// </summary>
        /// <param name="item">待添加元素。</param>
        public void Add(T item)
        {
            _items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        /// <summary>
        /// 移除首个匹配元素。
        /// </summary>
        /// <param name="item">待移除元素。</param>
        /// <returns>是否移除成功。</returns>
        public bool Remove(T item)
        {
            bool removed = _items.Remove(item);
            if (removed)
            {
                OnItemRemoved?.Invoke(item);
            }

            return removed;
        }

        /// <summary>
        /// 获取枚举器。
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// 获取非泛型枚举器。
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
