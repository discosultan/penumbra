#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using System;
using Microsoft.Xna.Framework;
using Scalar = System.Single;
#endif

namespace Penumbra.Mathematics.Collision2
{    
    public sealed class BoundingPolygon
    {
        public static ContainmentType ContainsExclusive(Vector2[] vertexes, Vector2 point)
        {
            ContainmentType result;
            ContainsExclusive(vertexes, ref point, out result);
            return result;
        }
        public static void ContainsExclusive(Vector2[] vertexes, ref Vector2 point, out ContainmentType result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes)); }
            int count = 0; //intersection count
            Vector2 v2;
            var v1 = vertexes[vertexes.Length - 1];
            for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
            {
                v2 = vertexes[index];
                var t1 = (v1.Y <= point.Y);
                if (t1 ^ (v2.Y <= point.Y))
                {
                    var temp = ((point.Y - v1.Y) * (v2.X - v1.X) - (point.X - v1.X) * (v2.Y - v1.Y));
                    if (t1) { if (temp > 0) { count++; } }
                    else { if (temp < 0) { count--; } }
                }
            }
            result = (count != 0) ? (ContainmentType.Contains) : (ContainmentType.Disjoint);
        }

        public static ContainmentType ContainsInclusive(Vector2[] vertexes, Vector2 point)
        {
            ContainmentType result;
            ContainsInclusive(vertexes, ref point, out result);
            return result;
        }
        public static void ContainsInclusive(Vector2[] vertexes, ref Vector2 point, out ContainmentType result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes)); }
            int count = 0;    // the crossing count
            Vector2 v1 = vertexes[vertexes.Length - 1];
            Vector2 v2;
            for (int index = 0; index < vertexes.Length; index++, v1 = v2)
            {
                v2 = vertexes[index];
                if (((v1.Y <= point.Y) ^ (v2.Y <= point.Y)) ||
                    (v1.Y == point.Y) || (v2.Y == point.Y))
                {
                    Scalar xIntersection = (v1.X + ((point.Y - v1.Y) / (v2.Y - v1.Y)) * (v2.X - v1.X));
                    if (point.X < xIntersection) // P.X < intersect
                    {
                        ++count;
                    }
                    else if (xIntersection == point.X)
                    {
                        result = ContainmentType.Contains;
                        return;
                    }
                }
            }
            result = ((count & 1) != 0) ? (ContainmentType.Contains) : (ContainmentType.Disjoint); //true if odd.
        }

        public static bool Intersects(Vector2[] vertexes1, Vector2[] vertexes2)
        {
            bool result;
            Intersects(vertexes1, vertexes2, out result);
            return result;
        }
        public static void Intersects(Vector2[] vertexes1, Vector2[] vertexes2, out bool result)
        {
            if (vertexes1 == null) { throw new ArgumentNullException(nameof(vertexes1)); }
            if (vertexes2 == null) { throw new ArgumentNullException(nameof(vertexes2)); }
            if (vertexes1.Length < 2) { throw new ArgumentOutOfRangeException(nameof(vertexes1)); }
            if (vertexes2.Length < 2) { throw new ArgumentOutOfRangeException(nameof(vertexes2)); }

            Vector2 v2;
            var v1 = vertexes1[vertexes1.Length - 1];
            var v3 = vertexes2[vertexes2.Length - 1];
            result = false;
            for (int index1 = 0; index1 < vertexes1.Length; ++index1, v1 = v2)
            {
                v2 = vertexes1[index1];
                Vector2 v4;
                for (int index2 = 0; index2 < vertexes2.Length; ++index2, v3 = v4)
                {
                    v4 = vertexes2[index2];
                    result = LineSegment.Intersects(ref v1, ref v2, ref v3, ref v4);
                    if (result) { return; }
                }
            }
        }

        public static Scalar GetDistance(Vector2[] vertexes, Vector2 point)
        {
            Scalar result;
            GetDistance(vertexes, ref point, out result);
            return result;
        }
        /*public static void GetDistance(Vector2[] vertexes, ref Vector2 point, out Scalar result)
        {
            if (vertexes == null) { throw new ArgumentNullException("vertexes"); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException("vertexes"); }
            Scalar distance1, distance2;
            int nearestIndex = 0;
            Vector2.DistanceSq(ref point, ref vertexes[0], out distance1);
            for (int index = 1; index < vertexes.Length; ++index)
            {
                Vector2.DistanceSq(ref point, ref vertexes[index], out distance2);
                if (distance1 > distance2)
                {
                    nearestIndex = index;
                    distance1 = distance2;
                }
            }
            Vector2 prev = vertexes[(nearestIndex - 1 + vertexes.Length) % vertexes.Length];
            Vector2 good = vertexes[nearestIndex];
            Vector2 next = vertexes[(nearestIndex + 1) % vertexes.Length];
            LineSegment.GetDistance(ref prev, ref good, ref point, out distance1);
            LineSegment.GetDistance(ref good, ref next, ref point, out distance2);
            result = Math.Min(distance1, distance2);
            ContainmentType contains;
            ContainsExclusive(vertexes, ref point, out contains);
            if (contains == ContainmentType.Contains) { result = -result; }
        }*/
        public static void GetDistance(Vector2[] vertexes, ref Vector2 point, out Scalar result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes)); }
            int count = 0; //intersection count
            Vector2 v1 = vertexes[vertexes.Length - 1];
            Vector2 v2;
            Scalar goodDistance = Scalar.PositiveInfinity;
            for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
            {
                v2 = vertexes[index];
                bool t1 = (v1.Y <= point.Y);
                if (t1 ^ (v2.Y <= point.Y))
                {
                    Scalar temp = ((point.Y - v1.Y) * (v2.X - v1.X) - (point.X - v1.X) * (v2.Y - v1.Y));
                    if (t1) { if (temp > 0) { count++; } }
                    else { if (temp < 0) { count--; } }
                }
                Scalar distance;
                LineSegment.GetDistanceSq(ref v1, ref v2, ref point, out distance);
                if (distance < goodDistance) { goodDistance = distance; }
            }
            result = Calc.Sqrt(goodDistance);
            if (count != 0)
            {
                result = -result;
            }
        }


        /// <summary>
        /// Calculates the Centroid of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <returns>The Centroid of a polygon.</returns>
        /// <remarks>
        /// This is Also known as Center of Gravity/Mass.
        /// </remarks>
        public static Vector2 GetCentroid(Vector2[] vertexes)
        {
            Vector2 result;
            GetCentroid(vertexes, out result);
            return result;
        }
        /// <summary>
        /// Calculates the Centroid of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <param name="centroid">The Centroid of a polygon.</param>
        /// <remarks>
        /// This is Also known as Center of Gravity/Mass.
        /// </remarks>
        public static void GetCentroid(Vector2[] vertexes, out Vector2 centroid)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes), "There must be at least 3 vertexes"); }
            centroid = Vector2.Zero;
            Scalar temp;
            Scalar area = 0;
            Vector2 v1 = vertexes[vertexes.Length - 1];
            Vector2 v2;
            for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
            {
                v2 = vertexes[index];
                VectorUtil.Cross(ref v1, ref v2, out temp);
                area += temp;
                centroid.X += ((v1.X + v2.X) * temp);
                centroid.Y += ((v1.Y + v2.Y) * temp);
            }
            temp = 1 / (Math.Abs(area) * 3);
            centroid.X *= temp;
            centroid.Y *= temp;
        }
        /// <summary>
        /// Calculates the area of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <returns>the area.</returns>
        public static Scalar GetArea(Vector2[] vertexes)
        {
            Scalar result;
            GetArea(vertexes, out result);
            return result;
        }
        /// <summary>
        /// Calculates the area of a polygon.
        /// </summary>
        /// <param name="vertexes">The vertexes of the polygon.</param>
        /// <param name="result">the area.</param>
        public static void GetArea(Vector2[] vertexes, out Scalar result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes), "There must be at least 3 vertexes"); }
            Scalar area = 0;
            Scalar temp;
            Vector2 v1 = vertexes[vertexes.Length - 1];
            Vector2 v2;
            for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
            {
                v2 = vertexes[index];
                VectorUtil.Cross(ref v1, ref v2, out temp);
                area += temp;
            }
            result = Math.Abs(area * .5f);
        }

        public static Scalar GetPerimeter(Vector2[] vertexes)
        {
            Scalar result;
            GetPerimeter(vertexes, out result);
            return result;
        }
        public static void GetPerimeter(Vector2[] vertexes, out Scalar result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes), "There must be at least 3 vertexes"); }
            Vector2 v1 = vertexes[vertexes.Length - 1];
            Vector2 v2;
            result = 0;
            for (int index = 0; index < vertexes.Length; ++index, v1 = v2)
            {
                v2 = vertexes[index];
                Scalar dist;
                Vector2.Distance(ref v1, ref v2, out dist);
                result += dist;
            }
        }

        public static Scalar GetInertia(Vector2[] vertexes)
        {
            Scalar result;
            GetInertia(vertexes, out result);
            return result;
        }
        public static void GetInertia(Vector2[] vertexes, out Scalar result)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length == 0) { throw new ArgumentOutOfRangeException(nameof(vertexes)); }
            if (vertexes.Length == 1)
            {
                result = 0;
                return;
            }

            Scalar denom = 0;
            Scalar numer = 0;
            Vector2 v2;
            var v1 = vertexes[vertexes.Length - 1];
            for (int index = 0; index < vertexes.Length; index++, v1 = v2)
            {
                v2 = vertexes[index];
                Scalar a;
                Vector2.Dot(ref v2, ref v2, out a);
                Scalar b;
                Vector2.Dot(ref v2, ref v1, out b);
                Scalar c;
                Vector2.Dot(ref v1, ref v1, out c);
                Scalar d;
                VectorUtil.Cross(ref v1, ref v2, out d);
                d = Math.Abs(d);
                numer += d;
                denom += (a + b + c) * d;
            }
            result = denom / (numer * 6);
        }

        readonly Vector2[] vertexes;
        public BoundingPolygon(Vector2[] vertexes)
        {
            if (vertexes == null) { throw new ArgumentNullException(nameof(vertexes)); }
            if (vertexes.Length < 3) { throw new ArgumentOutOfRangeException(nameof(vertexes)); }
            this.vertexes = vertexes;
        }
        public Vector2[] Vertexes => vertexes;

        public Scalar Area
        {
            get
            {
                Scalar result;
                GetArea(vertexes, out result);
                return result;
            }
        }
        public Scalar Perimeter
        {
            get
            {
                Scalar result;
                GetPerimeter(vertexes, out result);
                return result;
            }
        }

        public Scalar GetDistance(Vector2 point)
        {
            Scalar result;
            GetDistance(vertexes, ref point, out result);
            return result;
        }
        public void GetDistance(ref Vector2 point, out Scalar result)
        {
            GetDistance(vertexes, ref point, out result);
        }

        public ContainmentType Contains(Vector2 point)
        {
            ContainmentType result;
            Contains(ref point, out result);
            return result;
        }
        public void Contains(ref Vector2 point, out ContainmentType result)
        {
            ContainsInclusive(vertexes, ref point, out result);
        }

        public ContainmentType Contains(BoundingCircle circle)
        {
            ContainmentType result;
            Contains(ref circle, out result);
            return result;
        }
        public void Contains(ref BoundingCircle circle, out ContainmentType result)
        {
            Scalar distance;
            GetDistance(ref circle.Position, out distance);
            distance += circle.Radius;
            if (distance <= 0)
            {
                result = ContainmentType.Contains;
            }
            else if (distance <= circle.Radius)
            {
                result = ContainmentType.Intersects;
            }
            else
            {
                result = ContainmentType.Disjoint;
            }
        }

        public ContainmentType Contains(BoundingRectangle rect)
        {
            ContainmentType result;
            Contains(ref rect, out result);
            return result;
        }
        public void Contains(ref BoundingRectangle rect, out ContainmentType result)
        {
            Contains(rect.Corners(), out result);
        }

        public ContainmentType Contains(BoundingPolygon polygon)
        {
            ContainmentType result;
            Contains(ref polygon, out result);
            return result;
        }
        public void Contains(ref BoundingPolygon polygon, out ContainmentType result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            Contains(polygon.vertexes, out result);
        }
        private void Contains(Vector2[] otherVertexes, out ContainmentType result)
        {
            ContainmentType contains;
            result = ContainmentType.Disjoint;
            for (int index = 0; index < vertexes.Length; ++index)
            {
                ContainsExclusive(otherVertexes, ref vertexes[index], out contains);
                if (contains == ContainmentType.Contains) { result = ContainmentType.Intersects; return; }
            }
            for (int index = 0; index < otherVertexes.Length && result != ContainmentType.Intersects; ++index)
            {
                ContainsInclusive(vertexes, ref otherVertexes[index], out contains);
                result |= contains;
            }
            if (result == ContainmentType.Disjoint)
            {
                bool test;
                Intersects(vertexes, otherVertexes, out test);
                if (test) { result = ContainmentType.Intersects; }
            }
        }

        public Scalar Intersects(Ray ray)
        {
            Scalar result;
            Intersects(ref ray, out result);
            return result;
        }
        public bool Intersects(BoundingRectangle rect)
        {
            bool result;
            Intersects(ref rect, out result);
            return result;
        }
        public bool Intersects(BoundingCircle circle)
        {
            bool result;
            Intersects(ref circle, out result);
            return result;
        }
        public bool Intersects(BoundingPolygon polygon)
        {
            bool result;
            Intersects(ref polygon, out result);
            return result;
        }

        public void Intersects(ref Ray ray, out Scalar result)
        {
            result = -1;
            for (int index = 0; index < vertexes.Length; ++index)
            {
                int index2 = (index + 1) % vertexes.Length;
                Scalar temp;
                LineSegment.Intersects(ref vertexes[index], ref vertexes[index2], ref ray, out temp);
                if (temp >= 0 && (result == -1 || temp < result))
                {
                    result = temp;
                }
            }
        }
        public void Intersects(ref BoundingRectangle rect, out bool result)
        {
            Intersects(vertexes, rect.Corners(), out result);
        }
        public void Intersects(ref BoundingCircle circle, out bool result)
        {
            result = false;
            for (int index = 0; index < vertexes.Length; ++index)
            {
                int index2 = (index + 1) % vertexes.Length;
                Scalar temp;
                LineSegment.GetDistance(ref vertexes[index], ref vertexes[index2], ref circle.Position, out temp);
                if (temp <= circle.Radius)
                {
                    result = true;
                    break;
                }
            }
        }
        public void Intersects(ref BoundingPolygon polygon, out bool result)
        {
            if (polygon == null) { throw new ArgumentNullException(nameof(polygon)); }
            Intersects(vertexes, polygon.vertexes, out result);
        }
    }
}