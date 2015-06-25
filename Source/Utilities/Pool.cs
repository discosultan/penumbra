using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal class Pool<T> where T : class, new()
    {
        private int _capacity;
        private int _increaseCapacityBy;
        private readonly Queue<T> _queue;        

        public Pool(int initialCapacity = 4)
        {
            _increaseCapacityBy = initialCapacity;
            _queue = new Queue<T>(initialCapacity);
            IncreasePool();
        }

        public T Fetch()
        {
            if (_queue.Count <= 0)
            {
                IncreasePool();
            }
            return _queue.Dequeue();
        }

        public void Release(T item)
        {
            _queue.Enqueue(item);
        }

        private void IncreasePool()
        {
            for (int i = 0; i < _increaseCapacityBy; ++i)
                _queue.Enqueue(new T());
            _capacity += _increaseCapacityBy;            
            _increaseCapacityBy *= 2;
            Logger.Write($"Increased pool size to {_capacity}");
        }
    }
}
