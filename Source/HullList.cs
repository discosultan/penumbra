using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Penumbra.Mathematics;
using Penumbra.Mathematics.Clipping;

namespace Penumbra
{
    internal class HullList
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly ObservableCollection<Hull> _hulls;        

        public HullList(ObservableCollection<Hull> hulls)
        {
            _hulls = hulls;
            //_hulls.CollectionChanged += HullsCollectionChanged;
        }

        public List<Hull> ResolvedHulls { get; } = new List<Hull>();

        public bool AnyDirty(HullComponentDirtyFlags flags)
        {
            for (int i = 0; i < _hulls.Count; i++)
            {                
                if (_hulls[i].AnyDirty(flags)) return true;
            }
            return false;
        }

        public bool LightIsInside(Light light)
        {
            for (int j = 0; j < ResolvedHulls.Count; j++)
            {
                if (light.IsInside(ResolvedHulls[j]))
                {
                    return true;
                }
            }
            return false;
        }
        
        private readonly Dictionary<int, Polygon> _mergedPolygonsMapping = new Dictionary<int, Polygon>();
        public void Resolve()
        {
            ResolvedHulls.Clear();
            _mergedPolygonsMapping.Clear();
            for (int i = 0; i < _hulls.Count; i++)
            {
                bool polygonMerged;
                bool didNotIntersect = true;
                Polygon polygon = GetPolygon(_hulls, i, out polygonMerged);

                for (int j = i + 1; j < _hulls.Count; j++)
                {
                    bool nextPolygonMerged;
                    Polygon nextPolygon = GetPolygon(_hulls, j, out nextPolygonMerged);
                    if (polygon.Intersects(nextPolygon))
                    {
                        var result = new Polygon();
                        Polygon.Union(polygon, nextPolygon, result);
                        _mergedPolygonsMapping.Add(j, result);
                        didNotIntersect = false;
                    }                    
                }
                if (didNotIntersect)
                {
                    ResolvedHulls.Add(polygonMerged ? new Hull(polygon) : _hulls[i]);
                }
            }            
        }

        private Polygon GetPolygon(ObservableCollection<Hull> hulls, int index, out bool merged)
        {
            Polygon result;
            merged = true;
            if (!_mergedPolygonsMapping.TryGetValue(index, out result))
            {
                result = hulls[index].TransformedPoints;
                merged = false;
            }
            return result;
        }

        public int Count => ResolvedHulls.Count;

        //private void HullsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {                
        //        // TODO: Not wise to recreate entire list always. Even if it is recreated, we should
        //        // TODO: pool hull wrappers and reuse em to prevent unnecessary heap allocation.
        //        case NotifyCollectionChangedAction.Remove:
        //        case NotifyCollectionChangedAction.Replace:
        //        case NotifyCollectionChangedAction.Reset:
        //            _wrappedHulls.Clear();
        //            goto case NotifyCollectionChangedAction.Add;
        //        case NotifyCollectionChangedAction.Add:
        //            PopulateList(e.NewItems);                    
        //            break;       
        //    }
        //    CollectionChanged?.Invoke(sender, e);
        //}

        //private void PopulateList(ICollection values)
        //{
        //    if (values == null) return;

        //    values.ForEach<Hull>(hull => hull.Parts.ForEach(_wrappedHulls.Add));            
        //    Logger.Write($"Added {values.Count} hulls to list");
        //}

        public Hull this[int index] => ResolvedHulls[index];
    }
}
