using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics.Clipping;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;

namespace Penumbra.Mathematics
{
    internal class Polygon : IEnumerable<Vector2>
    {
        private readonly List<Vector2> _list;

        public Polygon(WindingOrder windingOrder, int capacity = 4)
        {
            _list = new List<Vector2>(capacity);            
            WindingOrder = windingOrder;
        }

        public Polygon(Polygon polygon)
        {
            _list = new List<Vector2>(polygon._list);
            WindingOrder = polygon.WindingOrder;
        }

        public Polygon(List<Vector2> list, WindingOrder windingOrder)
        {
            _list = new List<Vector2>(list);
            WindingOrder = windingOrder;
        }

        public WindingOrder WindingOrder { get; private set; }

        public int Count => _list.Count;
        public void Reverse() => _list.Reverse();

        public Vector2 this[int indexer]
        {
            get { return _list[indexer]; }
            set { _list[indexer] = value; }
        }

        public void Add(Vector2 element) => _list.Add(element);
        public void Insert(int index, Vector2 item) => _list.Insert(index, item);
        public void Clear() => _list.Clear();
        public int IndexOf(Vector2 item) => _list.IndexOf(item);
        public void RemoveAt(int index) => _list.RemoveAt(index);

        public void EnsureWindingOrder(WindingOrder desired)
        {
            if (desired != WindingOrder)
            {
                Reverse();
                WindingOrder = desired;
            }
        }

        public static List<Polygon> Wrap(Polygon polygon)
        {
            return new List<Polygon> {polygon};
        }

        public static List<Polygon> FromList(List<List<Vector2>> lists, WindingOrder windingOrder)
        {
            return lists.Select(x => new Polygon(x, windingOrder)).ToList();
        }

        public void GetIndices(WindingOrder windingOrder, List<int> indices)
        {
            Triangulator.Triangulate(this, indices, WindingOrder.Clockwise);
            //Triangulator.Triangulate(
            //    points,
            //    WindingOrder.CounterClockwise,
            //    WindingOrder.Clockwise,
            //    WindingOrder.Clockwise,
            //    out outputPoints,
            //    out outputIndices);
            //            Points = outputPoints;
            //            Indices = outputIndices;
        }

        public static void Clip(Polygon subj, Polygon clip, out Polygon sln)
        {
            PolyClipError err;
            var newSln = YuPengClipper.Difference(subj, clip, out err);
            if (err == PolyClipError.None)
            {
                sln = new Polygon(newSln[0], WindingOrder.CounterClockwise);
                //sln.EnsureWindingOrder(WindingOrder.Clockwise);
            }
            else
            {
                Logger.Write($"Error clipping: {err}");
                sln = subj;
            }
        }

        public static List<Polygon> DecomposeIntoConvex(Polygon polygon)
        {
            if (polygon.IsConvex())            
                return Wrap(polygon);            

            return FromList(BayazitDecomposer.ConvexPartition(polygon), WindingOrder.CounterClockwise);
        }

        public static implicit operator List<Vector2>(Polygon polygon)
        {
            return polygon._list;
        }

        public Enumerator<Vector2> GetEnumerator()
        {
            return new Enumerator<Vector2>(_list);
        }

        IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
