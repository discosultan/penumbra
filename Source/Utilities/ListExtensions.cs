using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal static class ListExtensions
    {
        public static int PreviousIndex<TCollection, TElement>(this TCollection array, int index) where TCollection : IList<TElement>
        {
            if (--index < 0) index = array.Count - 1;
            return index;
        }

        public static TElement PreviousElement<TCollection, TElement>(this TCollection array, int index) where TCollection : IList<TElement>
        {
            return array[PreviousIndex<TCollection, TElement>(array, index)];
        }

        public static int NextIndex<TCollection, TElement>(this TCollection array, int index) where TCollection : IList<TElement>
        {
            return ++index % array.Count;
        }

        public static TElement NextElement<TCollection, TElement>(this TCollection array, int index) where TCollection : IList<TElement>
        {
            return array[NextIndex<TCollection, TElement>(array, index)];
        }

        public static void ShiftRight<TCollection, TElement>(this TCollection list, int numTimes = 1) where TCollection : IList<TElement>
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
    }
}
