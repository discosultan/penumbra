using Microsoft.Xna.Framework;

namespace Penumbra.Geometry
{
    // An axis aligned bounding rectangle.
    internal struct BoundingRectangle
    {
        public Vector2 Min;
        public Vector2 Max;

        public BoundingRectangle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public bool Intersects(BoundingRectangle other)
        {
            return Intersects(ref other);
        }

        public bool Intersects(ref BoundingRectangle other)
        {
            Vector2 d1, d2;
            Vector2.Subtract(ref other.Min, ref Max, out d1);
            Vector2.Subtract(ref Min, ref other.Max, out d2);

            if (d1.X > 0.0f || d1.Y > 0.0f)
                return false;

            if (d2.X > 0.0f || d2.Y > 0.0f)
                return false;

            return true;
        }

        public override string ToString()
        {
            return $"{nameof(Min)}:{Min} {nameof(Max)}:{Max}";
        }

        public static void Transform(ref BoundingRectangle bounds, ref Matrix transform, out BoundingRectangle result)
        {
            var c1 = new Vector2(bounds.Min.X, bounds.Max.Y);
            var c2 = bounds.Max;
            var c3 = new Vector2(bounds.Max.X, bounds.Min.Y);
            var c4 = bounds.Min;

            Vector2.Transform(ref c1, ref transform, out c1);
            Vector2.Transform(ref c2, ref transform, out c2);
            Vector2.Transform(ref c3, ref transform, out c3);
            Vector2.Transform(ref c4, ref transform, out c4);

            Vector2 min, max;

            Vector2.Min(ref c1, ref c2, out min);
            Vector2.Min(ref min, ref c3, out min);
            Vector2.Min(ref min, ref c4, out min);

            Vector2.Max(ref c1, ref c2, out max);
            Vector2.Max(ref max, ref c3, out max);
            Vector2.Max(ref max, ref c4, out max);

            result = new BoundingRectangle(min, max);
        }
    }
}