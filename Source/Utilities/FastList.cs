using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Penumbra.Utilities
{
    // Differs from List{T} by allowing direct access to the underlying array.
    // Modified to use fast clearing by default and removed read only collection interface implementation for
    // .NET 4.0 support.
    // ref: https://github.com/SiliconStudio/paradox/blob/master/sources/common/core/SiliconStudio.Core/Collections/FastList.cs
    public class FastList<T> : IList<T>
    {
        // Fields
        private const int DefaultCapacity = 4;
        private static readonly T[] EmptyArray;

        /// <summary>
        /// Gets the items.
        /// </summary>
        public T[] Items { get; private set; }

        public static implicit operator T[](FastList<T> collection)
        {
            return collection.Items;
        }

        // Methods
        static FastList()
        {
            EmptyArray = new T[0];
        }

        public FastList()
        {
            Items = EmptyArray;
        }

        public FastList(IEnumerable<T> collection)
        {
            var is2 = collection as ICollection<T>;
            if (is2 != null)
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
                    {
                        Add(enumerator.Current);
                    }
                }
            }
        }

        public FastList(int capacity)
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
                        {
                            Array.Copy(Items, 0, destinationArray, 0, Count);
                        }
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
            {
                EnsureCapacity(Count + 1);
            }
            Items[Count++] = item;
        }

        public void IncreaseCapacity(int index)
        {
            EnsureCapacity(Count + index);
            Count += index;
        }

        public void Clear()
        {
            Clear(true);
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < Count; j++)
                {
                    if (Items[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Count; i++)
            {
                if (comparer.Equals(Items[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(Items, 0, array, arrayIndex, Count);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Items, item, 0, Count);
        }

        public void Insert(int index, T item)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            if (index < Count)
            {
                Array.Copy(Items, index, Items, index + 1, Count - index);
            }
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
            {
                Array.Copy(Items, index + 1, Items, index, Count - index);
            }
            Items[Count] = default(T);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int Count { get; private set; }

        public T this[int index]
        {
            get { return Items[index]; }
            set
            {
                Items[index] = value;
            }
        }

        bool ICollection<T>.IsReadOnly => false;

        #endregion

        /// <summary>
        /// Clears this list with a fast-clear option.
        /// </summary>
        /// <param name="fastClear">if set to <c>true</c> this method only resets the count elements but doesn't clear items referenced already stored in the list.</param>
        public void Clear(bool fastClear)
        {
            if (!fastClear && Count > 0)
            {
                Array.Clear(Items, 0, Count);
            }
            Count = 0;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }

        public int BinarySearch(T item)
        {
            return BinarySearch(0, Count, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, Count, item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return Array.BinarySearch(Items, index, count, item, comparer);
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            Array.Copy(Items, index, array, arrayIndex, count);
        }

        public void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                int num = (Items.Length == 0) ? DefaultCapacity : (Items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                Capacity = num;
            }
        }

        public bool Exists(Predicate<T> match)
        {
            return (FindIndex(match) != -1);
        }

        public T Find(Predicate<T> match)
        {
            for (int i = 0; i < Count; i++)
            {
                if (match(Items[i]))
                {
                    return Items[i];
                }
            }
            return default(T);
        }

        public FastList<T> FindAll(Predicate<T> match)
        {
            var list = new FastList<T>();
            for (int i = 0; i < Count; i++)
            {
                if (match(Items[i]))
                {
                    list.Add(Items[i]);
                }
            }
            return list;
        }

        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, Count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, Count - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(Items[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (match(Items[i]))
                {
                    return Items[i];
                }
            }
            return default(T);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(Count - 1, Count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            int num = startIndex - count;
            for (int i = startIndex; i > num; i--)
            {
                if (match(Items[i]))
                {
                    return i;
                }
            }
            return -1;
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
            var is2 = collection as ICollection<T>;
            if (is2 != null)
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
                    {
                        Insert(index++, enumerator.Current);
                    }
                }
            }
        }

        public int LastIndexOf(T item)
        {
            if (Count == 0)
            {
                return -1;
            }
            return LastIndexOf(item, Count - 1, Count);
        }

        public int LastIndexOf(T item, int index)
        {
            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (Count == 0)
            {
                return -1;
            }
            return Array.LastIndexOf(Items, item, index, count);
        }

        public int RemoveAll(Predicate<T> match)
        {
            int index = 0;
            while ((index < Count) && !match(Items[index]))
            {
                index++;
            }
            if (index >= Count)
            {
                return 0;
            }
            int num2 = index + 1;
            while (num2 < Count)
            {
                while ((num2 < Count) && match(Items[num2]))
                {
                    num2++;
                }
                if (num2 < Count)
                {
                    Items[index++] = Items[num2++];
                }
            }
            Array.Clear(Items, index, Count - index);
            int num3 = Count - index;
            Count = index;
            return num3;
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0)
            {
                Count -= count;
                if (index < Count)
                {
                    Array.Copy(Items, index + count, Items, index, Count - index);
                }
                Array.Clear(Items, Count, count);
            }
        }

        public void Reverse()
        {
            Reverse(0, Count);
        }

        public void Reverse(int index, int count)
        {
            Array.Reverse(Items, index, count);
        }

        public void Sort()
        {
            Sort(0, Count, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        //public void Sort(Comparison<T> comparison)
        //{
        //    if (this._size > 0)
        //    {
        //        IComparer<T> comparer = new Array.FunctorComparer<T>(comparison);
        //        Array.Sort<T>(this.Items, 0, this._size, comparer);
        //    }
        //}

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            Array.Sort(Items, index, count, comparer);
        }

        public T[] ToArray()
        {
            var destinationArray = new T[Count];
            Array.Copy(Items, 0, destinationArray, 0, Count);
            return destinationArray;
        }

        public void TrimExcess()
        {
            var num = (int)(Items.Length * 0.9);
            if (Count < num)
            {
                Capacity = Count;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            for (int i = 0; i < Count; i++)
            {
                if (!match(Items[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // Properties

        // Nested Types

        #region Nested type: Enumerator

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>
        {
            private readonly FastList<T> list;
            private int index;
            private T current;

            internal Enumerator(FastList<T> list)
            {
                this.list = list;
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                FastList<T> fastList = list;
                if (index < fastList.Count)
                {
                    current = fastList.Items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                current = default(T);
                return false;
            }

            public T Current => current;

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }

        #endregion
    }
}
