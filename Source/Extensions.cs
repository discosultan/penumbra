using Penumbra.Utilities;
using System.Collections.Generic;

namespace Penumbra
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of collection elements.</typeparam>
        /// <param name="listInterface">The <see cref="IList{T}"/> to add elements to.</param>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="IList{T}"/>.
        /// The collection itself cannot be <c>null</c>, but it can contain elements that are
        /// <c>null</c>, if type <typeparamref name="T"/> is a reference type.
        /// </param>
        public static void AddRange<T>(this IList<T> listInterface, IEnumerable<T> collection)
        {
            // Use fast path in case of extended observable collection.
            if (listInterface is ExtendedObservableCollection<T> extendedObservableCollection)
            {
                extendedObservableCollection.AddRange(collection);
                return;
            }

            // Use fast path in case of list.
            if (listInterface is List<T> list)
            {
                list.AddRange(collection);
                return;
            }

            // Fallback to iterative add.
            foreach (T item in collection)
                listInterface.Add(item);
        }
    }
}
