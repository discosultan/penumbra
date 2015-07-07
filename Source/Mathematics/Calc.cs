using System;
using Microsoft.Xna.Framework;

namespace Penumbra.Mathematics
{
    internal static class Calc
    {
        public const float Epsilon = 1.192092896e-07f;

        public static float Atan2(float y, float x)
        {
            return (float) Math.Atan2(y, x);
        }

        public static float Sin(float a)
        {
            return (float) Math.Sin(a);
        }

        public static float Cos(float d)
        {
            return (float) Math.Cos(d);           
        }

        public static float Acos(float d)
        {
            return (float) Math.Acos(d);
        }

        public static float Sqrt(float d)
        {
            return (float) Math.Sqrt(d);
        }

        public static int Step(float y, float x)
        {
            return x >= y ? 1 : 0;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static void Cross(ref Vector2 a, ref Vector2 b, out float c)
        {
            c = a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>Positive number if point is left, negative if point is right, 
        /// and 0 if points are collinear.</returns>
        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Area(ref a, ref b, ref c);
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

        /// <summary>
        /// Determines if three vertices are collinear (ie. on a straight line)
        /// </summary>
        /// <param name="a">First vertex</param>
        /// <param name="b">Second vertex</param>
        /// <param name="c">Third vertex</param>
        /// <returns></returns>
        public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return Collinear(ref a, ref b, ref c, 0);
        }

        public static bool Collinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance)
        {
            return FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        /// <summary>
        /// Checks if a floating point Value is within a specified
        /// range of values (inclusive).
        /// </summary>
        /// <param name="value">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>True if the Value is within the range specified,
        /// false otherwise.</returns>
        public static bool FloatInRange(float value, float min, float max)
        {
            return (value >= min && value <= max);
        }

        public static bool NearEqual(float lhv, float rhv)
        {
            return Math.Abs(lhv - rhv) < float.Epsilon;
        }

        public static bool NearZero(float val)
        {
            return Math.Abs(val) < float.Epsilon;
        }
    }
}
