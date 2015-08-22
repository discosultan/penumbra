using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Penumbra.Utilities
{
    internal static class ListExtensions
    {
        public static int PreviousIndex<TElement>(this IList<TElement> array, int index)
        {
            if (--index < 0) index = array.Count - 1;
            return index;
        }

        public static TElement PreviousElement<TElement>(this IList<TElement> array, int index)
        {            
            return array[PreviousIndex(array, index)];
        }

        public static int NextIndex<TElement>(this IList<TElement> array, int index)
        {
            return ++index % array.Count;
        }

        public static TElement NextElement<TElement>(this IList<TElement> array, int index)
        {
            return array[NextIndex(array, index)];
        }

        public static void ShiftRight<TElement>(this IList<TElement> list, int numTimes = 1)
        {
            if (list.Count < 2) return;

            for (int i = 0; i < numTimes; i++)
            {
                TElement last = list[list.Count - 1];
                for (int j = list.Count - 1; j > 0; j--)
                {
                    list[j] = list[j - 1];
                }
                list[0] = last;
            }
        }

        /// <summary>
        /// Executes an action for each (casted) item of the given enumerable.
        /// </summary>
        /// <typeparam name="T">Type of the item value in the enumerable.</typeparam>
        /// <param name="source">Input enumerable to work on.</param>
        /// <param name="action">Action performed for each item in the enumerable.</param>
        /// <remarks>This extension method do not yield. It acts just like a foreach statement, and performs a cast to a typed enumerable in the middle.</remarks>
        public static void ForEach<T>(this IEnumerable source, Action<T> action)
        {
            source.Cast<T>().ForEach(action);
        }

        /// <summary>
        /// Executes an action for each item of the given enumerable.
        /// </summary>
        /// <typeparam name="T">Type of the item value in the enumerable.</typeparam>
        /// <param name="source">Input enumerable to work on.</param>
        /// <param name="action">Action performed for each item in the enumerable.</param>
        /// <remarks>This extension method do not yield. It acts just like a foreach statement.</remarks>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        public static IEnumerable<int> GetIndicesBetween(this IList list, int start, int end) // count must be > 0
        {            
            int count = list.Count;
            int numIndicesMinOne = end >= start ? end - start : count - start + end;
            int result = start;
            yield return result;
            for (int i = 1; i <= numIndicesMinOne; i++)
            {
                result = (result + 1) % count;
                yield return result;
            }
        }

        public static IEnumerable<int> GetIndicesBetweenBackward(this IList list, int start, int end) // count must be > 0
        {
            int count = list.Count;
            int numIndicesMinOne = start >= end ? start - end : count - end + start;
            int result = start;
            yield return result;
            for (int i = 1; i <= numIndicesMinOne; i++)
            {
                result--;
                if (result < 0)
                    result = count - 1;
                yield return result;
            }
        }
    }
}
