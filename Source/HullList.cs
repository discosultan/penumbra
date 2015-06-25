using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class HullList<T> : IEnumerable<T>
    {
        private readonly ObservableCollection<Hull> _hulls;
        private readonly IHullFactory<T> _factory;

        private readonly List<T> _wrappedHullParts;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public HullList(ObservableCollection<Hull> hulls, IHullFactory<T> factory)
        {
            _hulls = hulls;
            _factory = factory;
            _hulls.CollectionChanged += HullsCollectionChanged;

            _wrappedHullParts = new List<T>();            
            foreach (Hull hull in _hulls)
            {
                foreach (HullPart hullPart in hull.Parts)
                    _wrappedHullParts.Add(_factory.New(hullPart));                
            }
        }

        public int Count => _wrappedHullParts.Count;

        public bool AnyDirty(HullComponentDirtyFlags flags)
        {
            return _hulls.Any(hull => (hull.DirtyFlags & flags) != 0);
        }

        private void HullsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {                
                // TODO: Not wise to recreate entire list always. Even if it is recreated, we should
                // TODO: pool hull wrappers and reuse em to prevent unnecessary heap allocation.
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    _wrappedHullParts.Clear();
                    goto case NotifyCollectionChangedAction.Add;
                case NotifyCollectionChangedAction.Add:
                    PopulateList(e.NewItems);                    
                    break;       
            }
            CollectionChanged?.Invoke(sender, e);
        }

        private void PopulateList(IList values)
        {            
            foreach (var newItem in values)
            {
                var hull = (Hull) newItem;
                foreach (HullPart hullPart in hull.Parts)
                    _wrappedHullParts.Add(_factory.New(hullPart));
            }
            Logger.Write($"Added {values.Count} hulls to list");
        }

        public T this[int index] => _wrappedHullParts[index];

        public IEnumerator<T> GetEnumerator()
        {
            return _wrappedHullParts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
