using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Penumbra.Utilities
{
    // Adds an AddRange functionality to ObservableCollection. We want this so that we don't get
    // collection changed events raised after adding each item from a sequence.
    // Ref: http://blogs.msdn.com/b/nathannesbit/archive/2009/04/20/addrange-and-observablecollection.aspx
    internal class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        private static readonly PropertyChangedEventArgs CountPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Count));
        private static readonly PropertyChangedEventArgs ItemsPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Items));

        // We must not use NULL for changedItem param as ObservableCollection will throw for that.
        private static readonly object _dummy = new object();
        private static readonly NotifyCollectionChangedEventArgs DefaultNotifyCollectionChangedEventArgs
            = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _dummy, 0);

        public void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();

            // We need the starting index later.
            int startingIndex = Count;

            // Add the items directly to the inner collection. In case the framework's inner
            // implementation uses List{T} type, use its add range instead for better performance.
            Items.AddRange(items);

            // Now raise the changed events.
            OnPropertyChanged(CountPropertyChangedEventArgs);
            OnPropertyChanged(ItemsPropertyChangedEventArgs);

            // The event args require a list of changed items and a starting index. Since the type is
            // internal and the library does not care about args, we pass default args instead.
            OnCollectionChanged(DefaultNotifyCollectionChangedEventArgs);
        }
    }
}
