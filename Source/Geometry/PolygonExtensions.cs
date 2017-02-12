using Microsoft.Xna.Framework;
using Polygon = System.Collections.Generic.IList<Microsoft.Xna.Framework.Vector2>;

namespace Penumbra.Geometry
{
    internal static class PolygonExtensions
    {
        // Assuming the polygon is simple; determines whether the polygon is convex.
        // NOTE: It will also return false if the input contains colinear edges.
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
            if (polygon.Count < 3)
                return true;

            return (polygon.GetSignedArea() > 0.0f);
        }

        public static bool IsSimple(this Polygon polygon)
        {
            int polyCount = polygon.Count;
            for (int i = 0; i < polyCount; ++i)
            {
                Vector2 a1 = polygon[i];
                Vector2 a2 = polygon[(i + 1) % polyCount];
                var seg1 = new LineSegment(a1, a2);

                for (int j = 0; j < polyCount - 3; ++j)
                {
                    Vector2 b1 = polygon[(i + 2 + j) % polyCount];
                    Vector2 b2 = polygon[(i + 3 + j) % polyCount];

                    var seg2 = new LineSegment(b1, b2);

                    if (seg1.Intersects(seg2))
                        return false;
                }
            }
            return true;
        }

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

        public static void GetBounds(this Polygon polygon, out BoundingRectangle bounds)
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

            bounds = new BoundingRectangle(lowerBound, upperBound);
        }
    }
}
