using System;

namespace Penumbra.Mathematics
{
    internal static class Calc
    {
        //public const float Epsilon = 1.192092896e-07f;
        public const float Epsilon = 0.0001f;        

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
            return Math.Abs(lhv - rhv) < Epsilon;
        }

        public static bool NearZero(float val)
        {
            return Math.Abs(val) < Epsilon;
        }
    }
}
