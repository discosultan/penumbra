using System;
using Microsoft.Xna.Framework;
using Penumbra.Utilities;
using Polygon = System.Collections.Generic.IList<Microsoft.Xna.Framework.Vector2>;

namespace Penumbra.Geometry
{
    internal static class PolygonExtensions
    {
        /// <summary>
        /// Assuming the polygon is simple; determines whether the polygon is convex.
        /// NOTE: It will also return false if the input contains colinear edges.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsConvex(this Polygon polygon)
        {
            // Ensure the polygon is convex and the interior
            // is to the left of each edge.
            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < polyCount ? i + 1 : 0;
                Vector2 edge = polygon[i2] - polygon[i1];

                for (int j = 0; j < polyCount; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                        continue;

                    Vector2 r = polygon[j] - polygon[i1];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }

        public static bool IsCounterClockWise(this Polygon polygon)
        {
            //We just return true for lines
            if (polygon.Count < 3)
                return true;

            return (polygon.GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Check for edge crossings.
        /// </summary>
        /// <returns></returns>
        public static bool IsSimple(this Polygon polygon)
        {
            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; ++i)
            {
                //int iplus = (i + 1) % Count;
                Vector2 a1 = polygon[i];
                Vector2 a2 = polygon[(i + 1) % polyCount];
                var seg1 = new LineSegment(a1, a2);
                
                for (int j = 0; j < polyCount - 3; ++j)
                {
                    //int jplus = j + 1 % Count;
                    Vector2 b1 = polygon[(i + 2 + j) % polyCount];
                    Vector2 b2 = polygon[(i + 3 + j) % polyCount];

                    var seg2 = new LineSegment(b1, b2);

                    if (seg1.Intersects(seg2))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public static float GetSignedArea(this Polygon polygon)
        {
            int i;
            float area = 0;

            int polyCount = polygon.Count;
            for (i = 0; i < polyCount; i++)
            {
                int j = (i + 1) % polyCount;
                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }

            area *= 0.5f;
            return area;
        }

        public static void ReverseWindingOrder(this Polygon polygon)
        {            
            var temp = new Vector2[polygon.Count];
            polygon.CopyTo(temp, 0);

            polygon.Clear();
            polygon.Add(temp[0]);
            
            for (int i = 1; i < temp.Length; i++)
                polygon.Add(temp[temp.Length - i]);
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetCentroid(this Polygon polygon)
        {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = polygon[i];
                Vector2 p3 = i + 1 < polyCount ? polygon[i + 1] : polygon[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float d = VectorUtil.Cross(ref e1, ref e2);

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
        public static float GetRadius(this Polygon polygon)
        {
            float area = polygon.GetSignedArea();

            float radiusSqrd = area / MathHelper.Pi;
            if (radiusSqrd < 0)
                radiusSqrd *= -1;

            return (float)Math.Sqrt(radiusSqrd);
        }

        /// <summary>
        /// Winding number test for a point in a polygon.
        /// </summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>-1 if the winding number is zero and the point is outside
        /// the polygon, 1 if the point is inside the polygon, and 0 if the point
        /// is on the polygons edge.</returns>
        public static bool PointInPolygon(this Polygon polygon, ref Vector2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; i++)
            {
                // Get points
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[(i + 1) % polyCount];

                // Test if a point is directly on the edge
                Vector2 edge = p2 - p1;
                float area = VectorUtil.Area(ref p1, ref p2, ref point);
                if (area == 0f && Vector2.Dot(point - p1, edge) >= 0f && Vector2.Dot(point - p2, edge) <= 0f)
                    return true;

                // Test edge for intersection with ray from point
                if (p1.Y <= point.Y && p2.Y > point.Y && area > 0f)
                    ++wn;
                else if (p2.Y <= point.Y && area < 0f)
                    --wn;
            }

            return wn != 0;
        }

        public static bool Contains(this Polygon polygon, ref Vector2 point)
        {
            // Ref: http://stackoverflow.com/a/8721483/1466456
            int i, j;
            bool contains = false;
            int polyCount = polygon.Count;
            for (i = 0, j = polyCount - 1; i < polyCount; j = i++)
            {
                if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                    (point.X <
                        (polygon[j].X - polygon[i].X) *
                        (point.Y - polygon[i].Y) /
                        (polygon[j].Y - polygon[i].Y) +
                        polygon[i].X))
                {
                    contains = !contains;
                }
            }
            return contains;
        }

        /// <summary>
        /// Returns an AABB for vertex.
        /// </summary>
        /// <returns></returns>
        public static BoundingRectangle GetCollisionBox(this Polygon polygon)
        {
            Vector2 lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 upperBound = new Vector2(float.MinValue, float.MinValue);

            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; i++)
            {
                if (polygon[i].X < lowerBound.X)
                    lowerBound.X = polygon[i].X;
                if (polygon[i].X > upperBound.X)
                    upperBound.X = polygon[i].X;

                if (polygon[i].Y < lowerBound.Y)
                    lowerBound.Y = polygon[i].Y;
                if (polygon[i].Y > upperBound.Y)
                    upperBound.Y = polygon[i].Y;
            }

            return new BoundingRectangle(lowerBound, upperBound);
        }
    }
}
