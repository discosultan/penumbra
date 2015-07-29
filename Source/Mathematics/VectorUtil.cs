using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal static class VectorUtil
    {
        // NB! We are using inverted y axis (y runs from top to bottom).

        public static Vector2 Rotate90CW(Vector2 v)
        {
            //return new Vector2(-v.Y, v.X); // inverted y
            return new Vector2(v.Y, -v.X);
        }

        public static Vector2 Rotate90CCW(Vector2 v)
        {
            //return new Vector2(v.Y, -v.X); // inverted y
            return new Vector2(-v.Y, v.X);
        }

        // Assumes a polygon where no two edges intersect.
        public static Vector2 CalculateCentroid(this Vector2[] points)
        {
            float area = 0.0f;
            float cx = 0.0f;
            float cy = 0.0f;

            for (int i = 0; i < points.Length; i++)
            {
                var k = (i + 1) % (points.Length);
                var tmp = points[i].X * points[k].Y -
                            points[k].X * points[i].Y;
                area += tmp;
                cx += (points[i].X + points[k].X) * tmp;
                cy += (points[i].Y + points[k].Y) * tmp;
            }
            area *= 0.5f;
            cx *= 1.0f / (6.0f * area);
            cy *= 1.0f / (6.0f * area);

            return new Vector2(cx, cy);
        }

        //public static Vector2 Project(Vector2 v, Vector2 onto)
        //{
        //    return onto * (Vector2.Dot(onto, v) / onto.LengthSquared());
        //}

        //public static float ProjectLength(Vector2 v, Vector2 onto)
        //{
        //    return Vector2.Dot(onto, v) / onto.Length();
        //}

        public static Vector2 Rotate(Vector2 v, float angle)
        {
            float num = Calc.Cos(angle); 
            float num2 = Calc.Sin(angle);
            return new Vector2(v.X * num + v.Y * num2, -v.X * num2 + v.Y * num);
        }

        //TODO: rename
        public static bool Intersects(Vector2 dirMiddle, Vector2 dirTest, Vector2 dirTestAgainst)
        {
            float dot1 = Vector2.Dot(dirMiddle, dirTest);
            float dot2 = Vector2.Dot(dirMiddle, dirTestAgainst);
            return dot1 < dot2;
        }

        public static bool LineIntersect(ref Vector2 a0, ref Vector2 a1, ref Vector2 b0, ref Vector2 b1, out Vector2 result)
        {
            result = Vector2.Zero;
            float num = (a1.X - a0.X) * (b1.Y - b0.Y) - (a1.Y - a0.Y) * (b1.X - b0.X);
            if (num == 0f)
            {
                return false;
            }
            float num2 = ((a0.Y - b0.Y) * (b1.X - b0.X) - (a0.X - b0.X) * (b1.Y - b0.Y)) / num;
            if (num2 < 0f || num2 > 1f)
            {
                return false;
            }
            float num3 = ((a0.Y - b0.Y) * (a1.X - a0.X) - (a0.X - b0.X) * (a1.Y - a0.Y)) / num;
            if (num3 < 0f || num3 > 1f)
            {
                return false;
            }
            result = a0 + (a1 - a0) * num2;
            return true;
        }        
    }
}
