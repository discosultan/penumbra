using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal class DynamicArray<T>
    {
        private const int DefaultCapacity = 4;
        
        private T[] _items;

        public DynamicArray(int capacity = DefaultCapacity)
        {
            Capacity = capacity;
            _items = new T[Capacity];
        }

        public void Add(T item)
        {
            Count++;
            EnsureArrayLength();
            _items[Count - 1] = item;
        }

        public void AddRange(List<T> collection)
        {
            int arrayIndex = Count;
            Count += collection.Count;
            EnsureArrayLength();
            collection.CopyTo(_items, arrayIndex);
        }  

        public void Clear()
        {
            Count = 0;            
        }

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public T this[int index]
        {
            // TODO: Check against current COUNT
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public static implicit operator T[] (DynamicArray<T> array)
        {
            return array._items;
        }

        private void EnsureArrayLength()
        {            
            if (Capacity >= Count) return;

            if (Capacity == 0)
            {
                Capacity = 1;
                _items = new T[Capacity];
            }
            else
            {
                do
                {
                    Capacity *= 2;
                } while (Capacity < Count);

                T[] oldArray = _items;
                _items = new T[Capacity];
                oldArray.CopyTo(_items, 0);
            }
        }        
    }
}
