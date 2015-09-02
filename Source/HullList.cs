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
            int hullCount = ResolvedHulls.Count;
            for (int i = 0; i < hullCount; i++)
            {                
                if (light.IsInside(ResolvedHulls[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private readonly List<int> _resolvedIndices = new List<int>();    
        public void Resolve()
        {
            ResolvedHulls.Clear();
            ResolvedHulls.AddRange(_hulls);
            return; // TODO: TEMP;

            _resolvedIndices.Clear();
            for (int i = 0; i < _hulls.Count; i++)
            {
                if (_resolvedIndices.Contains(i)) continue;

                _resolvedIndices.Add(i);
                Polygon poly = _hulls[i].WorldPoints;
                Polygon result;
                bool mergedPolygons = ResolvePolygon(poly, out result);

                ResolvedHulls.Add(mergedPolygons ? new Hull(result) : _hulls[i]);
            }          
        }

        private bool ResolvePolygon(Polygon polygon, out Polygon result)
        {
            result = polygon;
            bool mergedPolygons = false;
            for (int i = 1; i < _hulls.Count; i++)
            {
                if (_resolvedIndices.Contains(i)) continue;
                
                Polygon otherPolygon = _hulls[i].WorldPoints;
                if (polygon.Intersects(otherPolygon))
                {
                    _resolvedIndices.Add(i);

                    result = new Polygon();
                    Polygon.Union(polygon, otherPolygon, result);
                    mergedPolygons = true;
                    ResolvePolygon(result, out result);
                }
            }
            return mergedPolygons;
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
