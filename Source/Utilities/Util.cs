using System;
using System.Collections.Generic;

namespace Penumbra.Utilities
{
    internal static class Util
    {
        public static void Dispose<T>(T obj) where T : class, IDisposable
        {
            obj?.Dispose();
        }

        public static void Dispose<T>(Lazy<T> lazyObj) where T : class, IDisposable
        {
            if (lazyObj == null || !lazyObj.IsValueCreated) return;
            lazyObj.Value.Dispose();
        }

        public static int GetPreviousIndexFrom<T>(this IList<T> array, int index)
        {
            if (--index < 0) index = array.Count - 1;
            return index;
        }

        public static T GetPreviousFrom<T>(this IList<T> array, int index)
        {
            return array[GetPreviousIndexFrom(array, index)];
        }

        public static int GetNextIndexFrom<T>(this IList<T> array, int index)
        {
            return ++index % array.Count;
        }

        public static T GetNextFrom<T>(this IList<T> array, int index)
        {
            return array[GetNextIndexFrom(array, index)];                        
        }
    }
}
