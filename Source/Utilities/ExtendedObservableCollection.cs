using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Penumbra.Utilities
{    
    // Adds an AddRange functionality to <see cref="ObservableCollection{T}"/>. We want this so that we don't get
    // collection changed events raised after adding each item from a sequence.
    // Ref: http://blogs.msdn.com/b/nathannesbit/archive/2009/04/20/addrange-and-observablecollection.aspx    
    internal class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public void AddRange(IEnumerable<T> items)
        {                        
            CheckReentrancy();
            //
            // We need the starting index later.
            //
            int startingIndex = Count;

            //
            // Add the items directly to the inner collection.
            //
            var changedItems = items.ToList();
            foreach (var data in changedItems)
            {                
                Items.Add(data);
            }
            
            //
            // Now raise the changed events.
            //
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Items)));

            //
            // We have to change our input of new items into an IList since that is what the
            // event args require.
            //                      
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startingIndex));
        }        
    }
}
