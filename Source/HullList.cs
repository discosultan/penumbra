using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Penumbra.Utilities;

namespace Penumbra
{
    internal class HullList
    {
        private readonly ObservableCollection<Hull> _hulls;

        private readonly List<HullPart> _wrappedHullParts;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public HullList(ObservableCollection<Hull> hulls)
        {
            _hulls = hulls;
            _hulls.CollectionChanged += HullsCollectionChanged;

            _wrappedHullParts = new List<HullPart>();            
            foreach (Hull hull in _hulls)
            {                
                _wrappedHullParts.AddRange(hull.Parts);
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
            if (values == null) return;

            foreach (var newItem in values)
            {
                var hull = (Hull) newItem;
                foreach (HullPart hullPart in hull.Parts)
                    _wrappedHullParts.Add(hullPart);
            }
            Logger.Write($"Added {values.Count} hulls to list");
        }

        public HullPart this[int index] => _wrappedHullParts[index];

        public Enumerator<HullPart> GetEnumerator()
        {
            return new Enumerator<HullPart>(_wrappedHullParts);
        }
    }
}
