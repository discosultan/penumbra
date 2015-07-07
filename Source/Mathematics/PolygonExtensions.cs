using Microsoft.Xna.Framework;
//using System.Collections.Generic;
//using Polygon = System.Collections.Generic.List<Microsoft.Xna.Framework.Vector2>;

namespace Penumbra.Mathematics
{
    internal static class PolygonExtensions
    {
        public static int PreviousIndex(this Polygon array, int index)
        {
            if (--index < 0) index = array.Count - 1;
            return index;
        }

        public static Vector2 PreviousElement(this Polygon array, int index)
        {
            return array[PreviousIndex(array, index)];
        }

        public static int NextIndex(this Polygon array, int index)
        {
            return ++index % array.Count;
        }

        public static Vector2 NextElement(this Polygon array, int index)
        {
            return array[NextIndex(array, index)];
        }

        /// <summary>
        /// Gets the signed area.
        /// </summary>
        /// <returns></returns>
        public static float GetSignedArea(this Polygon polygon)
        {
            int i;
            float area = 0;

            for (i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>
        /// Gets the area.
        /// </summary>
        /// <returns></returns>
        public static float GetArea(this Polygon polygon)
        {
            int i;
            float area = 0;

            for (i = 0; i < polygon.Count; i++)
            {
                int j = (i + 1) % polygon.Count;
                area += polygon[i].X * polygon[j].Y;
                area -= polygon[i].Y * polygon[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
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
            for (int i = 0; i < polygon.Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = polygon[i];
                Vector2 p3 = i + 1 < polygon.Count ? polygon[i + 1] : polygon[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = Calc.Cross(e1, e2);

                float triangleArea = 0.5f * D;
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
            {
                radiusSqrd *= -1;
            }

            return Calc.Sqrt(radiusSqrd);
        }



        public static void Translate(this Polygon polygon, Vector2 vector)
        {
            polygon.Translate(ref vector);
        }

        /// <summary>
        /// Translates the vertices with the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public static void Translate(this Polygon polygon, ref Vector2 vector)
        {
            for (int i = 0; i < polygon.Count; i++)
                polygon[i] = Vector2.Add(polygon[i], vector);
        }

        /// <summary>
        /// Scales the vertices with the specified vector.
        /// </summary>
        /// <param name="value">The Value.</param>
        public static void Scale(this Polygon polygon, ref Vector2 value)
        {
            for (int i = 0; i < polygon.Count; i++)
                polygon[i] = Vector2.Multiply(polygon[i], value);
        }

        /// <summary>
        /// Rotate the vertices with the defined value in radians.
        /// </summary>        
        /// <param name="value">The amount to rotate by in radians.</param>
        public static void Rotate(this Polygon polygon, float value)
        {
            Matrix rotationMatrix;
            Matrix.CreateRotationZ(value, out rotationMatrix);

            for (int i = 0; i < polygon.Count; i++)
                polygon[i] = Vector2.Transform(polygon[i], rotationMatrix);
        }

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
            for (int i = 0; i < polygon.Count; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < polygon.Count ? i + 1 : 0;
                Vector2 edge = polygon[i2] - polygon[i1];

                for (int j = 0; j < polygon.Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                    {
                        continue;
                    }

                    Vector2 r = polygon[j] - polygon[i1];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }        

        /// <summary>
        /// Check for edge crossings
        /// </summary>
        /// <returns></returns>
        public static bool IsSimple(this Polygon polygon)
        {
            for (int i = 0; i < polygon.Count; ++i)
            {
                int iplus = (i + 1 > polygon.Count - 1) ? 0 : i + 1;
                Vector2 a1 = new Vector2(polygon[i].X, polygon[i].Y);
                Vector2 a2 = new Vector2(polygon[iplus].X, polygon[iplus].Y);
                for (int j = i + 1; j < polygon.Count; ++j)
                {
                    int jplus = (j + 1 > polygon.Count - 1) ? 0 : j + 1;
                    Vector2 b1 = new Vector2(polygon[j].X, polygon[j].Y);
                    Vector2 b2 = new Vector2(polygon[jplus].X, polygon[jplus].Y);

                    Vector2 temp;

                    if (LineTools.LineIntersect2(a1, a2, b1, b2, out temp))
                    {
                        return false;
                    }
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
        public static IntersectionResult PointInPolygon(this Polygon polygon, ref Vector2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < polygon.Count; i++)
            {
                // Get points
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon.NextElement(i);

                // Test if a point is directly on the edge
                Vector2 edge = p2 - p1;
                float area = Calc.Area(ref p1, ref p2, ref point);
                if (area == 0f && Vector2.Dot(point - p1, edge) >= 0f && Vector2.Dot(point - p2, edge) <= 0f)
                {
                    return IntersectionResult.PartiallyContained;
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
            return (wn == 0 ? IntersectionResult.None : IntersectionResult.FullyContained);
        }

        /// <summary>
        /// Returns an AABB for vertex.
        /// </summary>
        /// <returns></returns>
        public static AABB GetCollisionBox(this Polygon polygon)
        {
            AABB aabb;
            Vector2 lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 upperBound = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < polygon.Count; ++i)
            {
                if (polygon[i].X < lowerBound.X)
                {
                    lowerBound.X = polygon[i].X;
                }
                if (polygon[i].X > upperBound.X)
                {
                    upperBound.X = polygon[i].X;
                }

                if (polygon[i].Y < lowerBound.Y)
                {
                    lowerBound.Y = polygon[i].Y;
                }
                if (polygon[i].Y > upperBound.Y)
                {
                    upperBound.Y = polygon[i].Y;
                }
            }

            aabb.LowerBound = lowerBound;
            aabb.UpperBound = upperBound;

            return aabb;
        }
    }
}
