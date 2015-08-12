using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Penumbra.Mathematics.Clipping;
using Penumbra.Mathematics.Triangulation;
using Penumbra.Utilities;
using System.Collections;
using FarseerPhysics.Common;
using Penumbra.Mathematics.Collision2;
using Penumbra.Mathematics.Geometry;
using BoundingRectangle = Penumbra.Mathematics.Collision.BoundingRectangle;
using LineSegment2D = Penumbra.Mathematics.Geometry.LineSegment2D;

namespace Penumbra.Mathematics
{
    internal class Polygon : IList<Vector2>
    {        
        private readonly FastList<Vector2> _list;

        public Polygon(int capacity = 4)
        {
            _list = new FastList<Vector2>(capacity);            
            WindingOrder = IsCounterClockWise() ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
        }

        public Polygon(IEnumerable<Vector2> list)
        {
            _list = new FastList<Vector2>(list);
            WindingOrder = IsCounterClockWise() ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
        }

        public WindingOrder WindingOrder { get; private set; }

        public int Count => _list.Count;        

        public void Reverse() => _list.Reverse();

        public Vector2 this[int indexer]
        {
            get { return _list[indexer]; }
            set { _list[indexer] = value; }
        }

        public void Add(Vector2 element)
        {
            _list.Add(element);            
        }
        //public void AddRange(IEnumerable<Vector2> range) => _list.AddRange(range);
        public void AddRange(Polygon polygon)
        {
            _list.AddRange(polygon._list);                              
        }

        public void Insert(int index, Vector2 item)
        {
            _list.Insert(index, item);            
        }
        public void Clear()
        {
            _list.Clear();            
        }
        public int IndexOf(Vector2 item) => _list.IndexOf(item);
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);            
        }

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
        
        public void GetIndices(WindingOrder windingOrder, List<int> indices)
        {
            if (IsConvex())
            {
                int numTriangles = Count - 2;
                if (windingOrder == WindingOrder.Clockwise)
                {
                    for (int i = numTriangles - 1; i >= 0; i--)
                    {
                        indices.Add(0);
                        indices.Add(i + 2);
                        indices.Add(i + 1);
                    }
                }
                else
                {                    
                    for (int i = 0; i < numTriangles; i++)
                    {
                        indices.Add(0);
                        indices.Add(i + 1);
                        indices.Add(i + 2);
                    }
                }
            }
            else
            {
                //_triangles.Clear();
                Triangulator.Process(this, indices, windingOrder);
                //for (int i = 0; i < _triangles.Count; i+=3)
                //{
                //    indices.Add()
                //}

                //Triangulator.Triangulate(this, indices, windingOrder);
            }
        }        

        private static readonly List<Polygon> ClippingSolutions = new List<Polygon>();
        public static void Clip(Polygon subj, Polygon clip, Polygon result)
        {
            int numSln;
            PolyClipError err = YuPengClipper.Difference(subj, clip, ClippingSolutions, out numSln);            
            if (err == PolyClipError.None)
            {
                result.Clear();
                result.AddRange(ClippingSolutions[0]);
                return;
            }            
            Logger.Write($"Error clipping: {err}");
        }

        public static void Union(Polygon subj, Polygon clip, Polygon result)
        {
            AngusClipper.Union(subj, clip, result);

            //int numSln;
            //PolyClipError err = YuPengClipper.Union(subj, clip, ClippingSolutions, out numSln);
            //if (err == PolyClipError.None)
            //{
            //    result.Clear();
            //    result.AddRange(ClippingSolutions[0]);
            //    return;
            //}

            //Logger.Write($"Error clipping: {err}");

            //var vertices1 = new Vertices(subj);
            //var vertices2 = new Vertices(clip);
            //FarseerPhysics.Common.PolygonManipulation.PolyClipError err2;
            //var resultVertices = FarseerPhysics.Common.PolygonManipulation.YuPengClipper.Union(vertices1, vertices2,
            //    out err2);
            //Logger.Write($"Error clipping: {err2}");

            //result = new Polygon(resultVertices[0]);
        }

        public static List<Polygon> DecomposeIntoConvex(Polygon polygon)
        {
            if (polygon.IsConvex())
                return Wrap(polygon);            

            return BayazitDecomposer.ConvexPartition(polygon);
        }

        public FastList<Vector2>.Enumerator GetEnumerator() => new FastList<Vector2>.Enumerator(_list);
        IEnumerator<Vector2> IEnumerable<Vector2>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();        

        public bool Contains(Vector2 item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Vector2[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(Vector2 item)
        {            
            return _list.Remove(item);            
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public float GetSignedArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area *= 0.5f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            float area = GetSignedArea();
            return (area < 0 ? -area : area);
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCentroid()
        {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            for (int i = 0; i < Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = this[i];
                Vector2 p3 = i + 1 < Count ? this[i + 1] : this[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float d = VectorUtil.Cross(e1, e2);

                float triangleArea = 0.5f * d;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            c *= 1.0f / area;
            return c;
        }

        /// <summary>
        /// Gets the radius based on area.
        /// </summary>
        /// <returns></returns>
        public float GetRadius()
        {
            float area = GetSignedArea();

            float radiusSqrd = area / MathHelper.Pi;
            if (radiusSqrd < 0)
            {
                radiusSqrd *= -1;
            }

            return Calc.Sqrt(radiusSqrd);
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Add(this[i], vector);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Multiply(this[i], value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// </summary>        
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Transform(this[i], rotationMatrix);
        }

        /// <summary>
        /// Assuming the polygon is simple; determines whether the polygon is convex.
        /// NOTE: It will also return false if the input contains colinear edges.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex()
        {              
            // Ensure the polygon is convex and the interior
            // is to the left of each edge.
            for (int i = 0; i < Count; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < Count ? i + 1 : 0;
                Vector2 edge = this[i2] - this[i1];

                for (int j = 0; j < Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                    {
                        continue;
                    }

                    Vector2 r = this[j] - this[i1];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                    {
                        return false;
                    }
                }
            }
            return true;                      
        }

        public bool IsCounterClockWise()
        {
            //We just return true for lines
            if (Count < 3)
                return true;

            return (GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Check for edge crossings.
        /// </summary>
        /// <returns></returns>
        public bool IsSimple()
        {
            for (int i = 0; i < Count; ++i)
            {                
                //int iplus = (i + 1) % Count;
                Vector2 a1 = this[i];
                Vector2 a2 = this[(i + 1) % Count];
                var seg1 = new LineSegment2D(a1, a2);

                for (int j = 0; j < Count - 3; ++j)
                {                    
                    //int jplus = j + 1 % Count;
                    Vector2 b1 = this[(i + 2 + j) % Count];
                    Vector2 b2 = this[(i + 3 + j) % Count];
                    
                    var seg2 = new LineSegment2D(b1, b2);                    

                    if (seg1.Intersects(seg2))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Winding number test for a point in a polygon.
        /// </summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>-1 if the winding number is zero and the point is outside
        /// the polygon, 1 if the point is inside the polygon, and 0 if the point
        /// is on the polygons edge.</returns>
        public bool PointInPolygon(ref Vector2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < Count; i++)
            {
                // Get points
                Vector2 p1 = this[i];
                Vector2 p2 = this.NextElement(i);

                // Test if a point is directly on the edge
                Vector2 edge = p2 - p1;
                float area = VectorUtil.Area(ref p1, ref p2, ref point);
                if (area == 0f && Vector2.Dot(point - p1, edge) >= 0f && Vector2.Dot(point - p2, edge) <= 0f)
                {
                    //return IntersectionResult.PartiallyContained;
                    return true;
                }
                // Test edge for intersection with ray from point
                if (p1.Y <= point.Y)
                {
                    if (p2.Y > point.Y && area > 0f)
                    {
                        ++wn;
                    }
                }
                else
                {
                    if (p2.Y <= point.Y && area < 0f)
                    {
                        --wn;
                    }
                }
            }
            //return (wn == 0 ? IntersectionResult.None : IntersectionResult.FullyContained);
            return wn != 0;
        }        

        //public static bool PointIsInside(Vector2[] points, Vector2 point)
        //{
        //    // Ref: http://stackoverflow.com/a/8721483/1466456
        //    int i, j;
        //    bool contains = false;
        //    for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
        //    {
        //        if ((points[i].Y > point.Y) != (points[j].Y > point.Y) &&
        //            (point.X <
        //                (points[j].X - points[i].X) *
        //                (point.Y - points[i].Y) /
        //                (points[j].Y - points[i].Y) +
        //                points[i].X))
        //        {
        //            contains = !contains;
        //        }
        //    }
        //    return contains;
        //}

        /// <summary>
        /// Returns an AABB for vertex.
        /// </summary>
        /// <returns></returns>
        public BoundingRectangle GetCollisionBox()
        {
            Vector2 lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 upperBound = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < Count; ++i)
            {
                if (this[i].X < lowerBound.X)
                {
                    lowerBound.X = this[i].X;
                }
                if (this[i].X > upperBound.X)
                {
                    upperBound.X = this[i].X;
                }

                if (this[i].Y < lowerBound.Y)
                {
                    lowerBound.Y = this[i].Y;
                }
                if (this[i].Y > upperBound.Y)
                {
                    upperBound.Y = this[i].Y;
                }
            }

            return new BoundingRectangle(ref lowerBound, ref upperBound);
        }

        // TODO: impr perf
        public bool Intersects(Polygon other)
        {
            var p1 = new BoundingPolygon(_list.ToArray());
            var p2 = new BoundingPolygon(other._list.ToArray());

            return p1.Intersects(p2);

            //for (int j = Count - 1, i = 0; i < Count; j = i, i++)
            //{
            //    var segment = new LineSegment2D(this[i], this[j]);
            //    for (int l = other.Count - 1, k = 0; k < other.Count; l = k, k++)
            //    {
            //        var otherSegment = new LineSegment2D(other[k], other[l]);
            //        if (segment.Intersects(ref otherSegment))
            //        {
            //            return true;
            //        }
            //    }                 
            //}

            //return false;
        }

        public override string ToString()
        {
            return string.Join(" ", _list);
        }

        bool ICollection<Vector2>.IsReadOnly => ((IList<Vector2>)_list).IsReadOnly;
    }
}
