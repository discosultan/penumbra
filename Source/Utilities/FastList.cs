using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Penumbra.Utilities
{
    // Differs from List{T} by allowing direct access to the underlying array.
    // Ref: https://github.com/SiliconStudio/paradox/blob/master/sources/common/core/SiliconStudio.Core/Collections/FastList.cs
    internal class FastList<T> : IList<T>
    {
        // Fields
        private const int DefaultCapacity = 4;
        private static readonly T[] EmptyArray = new T[0];

        public T[] Items { get; private set; }

        public static implicit operator T[](FastList<T> collection) => collection.Items;

        public FastList(IEnumerable<T> collection)
        {
            if (collection is ICollection<T> is2)
            {
                int count = is2.Count;
                Items = new T[count];
                is2.CopyTo(Items, 0);
                Count = count;
            }
            else
            {
                Count = 0;
                Items = new T[DefaultCapacity];
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        Add(enumerator.Current);
                }
            }
        }

        public FastList(int capacity = DefaultCapacity)
        {
            Items = new T[capacity];
        }

        public int Capacity
        {
            get { return Items.Length; }
            set
            {
                if (value != Items.Length)
                {
                    if (value > 0)
                    {
                        var destinationArray = new T[value];
                        if (Count > 0)
                            Array.Copy(Items, 0, destinationArray, 0, Count);
                        Items = destinationArray;
                    }
                    else
                    {
                        Items = EmptyArray;
                    }
                }
            }
        }

        #region IList<T> Members

        public void Add(T item)
        {
            if (Count == Items.Length)
                EnsureCapacity(Count + 1);
            Items[Count++] = item;
        }

        public void IncreaseCapacity(int index)
        {
            EnsureCapacity(Count + index);
            Count += index;
        }

        public void Clear() => Clear(true);

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < Count; j++)
                    if (Items[j] == null)
                        return true;
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
                if (comparer.Equals(Items[i], item))
                    return true;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) => Array.Copy(Items, 0, array, arrayIndex, Count);

        public int IndexOf(T item) => Array.IndexOf(Items, item, 0, Count);

        public void Insert(int index, T item)
        {
            if (Count == Items.Length)
                EnsureCapacity(Count + 1);
            if (index < Count)
                Array.Copy(Items, index, Items, index + 1, Count - index);
            Items[index] = item;
            Count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            Count--;
            if (index < Count)
                Array.Copy(Items, index + 1, Items, index, Count - index);
            Items[Count] = default(T);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public int Count { get; private set; }

        public T this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        bool ICollection<T>.IsReadOnly => false;

        #endregion

        public void Clear(bool fastClear)
        {
            if (!fastClear && Count > 0)
                Array.Clear(Items, 0, Count);
            Count = 0;
        }

        public void AddRange(IEnumerable<T> collection) => InsertRange(Count, collection);

        public void CopyTo(T[] array) => CopyTo(array, 0);

        public void CopyTo(int index, T[] array, int arrayIndex, int count) =>
            Array.Copy(Items, index, array, arrayIndex, count);

        private void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                int num = (Items.Length == 0) ? DefaultCapacity : (Items.Length * 2);
                if (num < min)
                    num = min;
                Capacity = num;
            }
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < Count; i++)
            {
                action(Items[i]);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public FastList<T> GetRange(int index, int count)
        {
            var list = new FastList<T>(count);
            Array.Copy(Items, index, list.Items, 0, count);
            list.Count = count;
            return list;
        }

        public int IndexOf(T item, int index)
        {
            return Array.IndexOf(Items, item, index, Count - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            return Array.IndexOf(Items, item, index, count);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection is ICollection<T> is2)
            {
                int count = is2.Count;
                if (count > 0)
                {
                    EnsureCapacity(Count + count);
                    if (index < Count)
                    {
                        Array.Copy(Items, index, Items, index + count, Count - index);
                    }
                    if (this == is2)
                    {
                        Array.Copy(Items, 0, Items, index, index);
                        Array.Copy(Items, (index + count), Items, (index * 2), (Count - index));
                    }
                    else
                    {
                        is2.CopyTo(Items, index);
                    }
                    Count += count;
                }
            }
            else
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                        Insert(index++, enumerator.Current);
                }
            }
        }

        // Properties

        // Nested Types

        #region Nested type: Enumerator

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly FastList<T> list;
            private int index;

            internal Enumerator(FastList<T> list)
            {
                this.list = list;
                index = 0;
                Current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                FastList<T> fastList = list;
                if (index < fastList.Count)
                {
                    Current = fastList.Items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                Current = default(T);
                return false;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                index = 0;
                Current = default(T);
            }
        }

        #endregion
    }
}
