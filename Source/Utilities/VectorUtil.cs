using System;
using Microsoft.Xna.Framework;

namespace Penumbra.Utilities
{
    internal static class VectorUtil
    {
        public static float Cross(ref Vector2 a, ref Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static void Rotate(ref Vector2 v, float angle, out Vector2 result)
        {
            double num = Math.Cos(angle);
            double num2 = Math.Sin(angle);
            result = new Vector2((float)(v.X * num + v.Y * num2), (float)(-v.X * num2 + v.Y * num));
        }
    }
}
