using Microsoft.Xna.Framework;

namespace Penumbra.Utilities
{
    internal static class VectorUtil
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
