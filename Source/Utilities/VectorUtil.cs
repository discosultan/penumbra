using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal static class VectorUtil
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>Positive number if point is left, negative if point is right, 
        /// and 0 if points are collinear.</returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
        }
    }
}
