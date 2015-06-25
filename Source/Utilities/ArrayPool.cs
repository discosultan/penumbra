using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal class ArrayPool<T>
    {
        private const int DefaultQueueCapacity = 1; // TODO: we might want to reimplement how the pool size is increased.

        private readonly Dictionary<int, Queue<T[]>> _queues = new Dictionary<int,Queue<T[]>>();

        public T[] Fetch(int arraySize)
        {
            Queue<T[]> queue;
            if (!_queues.TryGetValue(arraySize, out queue))
            {
                queue = new Queue<T[]>(DefaultQueueCapacity);                
                _queues.Add(arraySize, queue);
            }

            if (queue.Count <= 0)
            {
                IncreasePool(queue, arraySize);
            }
            return queue.Dequeue();
        }

        public void Release(T[] item)
        {
            _queues[item.Length].Enqueue(item);            
        }

        private void IncreasePool(Queue<T[]> queue, int arraySize)
        {
            for (int i = 0; i < DefaultQueueCapacity; ++i)
                queue.Enqueue(new T[arraySize]);
            Logger.Write($"Added new array for array size {arraySize} to pool.");
        }
    }

    internal static class ArrayPoolExtensions
    {
        public static T[] ToArrayFromPool<T>(this IList<T> source, ArrayPool<T> pool)
        {
            T[] destination = pool.Fetch(source.Count);            
            source.CopyTo(destination, 0);            
            return destination;
        }
    }
}
