using Microsoft.Xna.Framework;

namespace Penumbra.Geometry
{
    /// <summary>
    /// An axis aligned bounding rectangle.
    /// </summary>
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
            Vector2 d1 = other.Min - Max;
            Vector2 d2 = Min - other.Max;

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
    }
}