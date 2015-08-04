using System;

namespace Penumbra.Mathematics
{
    internal static class Calc
    {
        //public const float Epsilon = 1.192092896e-07f;
        public const float Epsilon = 0.0001f;
        public const float Pi = (float) Math.PI;

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

        public static void Sort(float value1, float value2, out float max, out float min)
        {
            if (value1 > value2)
            {
                max = value1;
                min = value2;
            }
            else
            {
                max = value2;
                min = value1;
            }
        }

        /// <summary>
        /// Trys to Solve for x in the equation: (a * (x * x) + b * x + c == 0)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="plus">The result of (b + Math.Sqrt((b * b) - (4 * a * c))) / (2 * a)</param>
        /// <param name="minus">The result of (b - Math.Sqrt((b * b) - (4 * a * c))) / (2 * a)</param>
        /// <returns><see langword="false" /> if an error would have been thrown; otherwise <see langword="true" />.</returns>
        public static bool TrySolveQuadratic(float a, float b, float c, out float plus, out float minus)
        {
            if (0 == a)
            {
                plus = -c / b;
                minus = plus;
                return true;
            }
            c = (b * b) - (4 * a * c);
            if (0 <= c)
            {
                c = Sqrt(c);
                a = .5f / a;
                plus = ((c - b) * a);
                minus = ((-c - b) * a);
                return true;
            }
            plus = 0;
            minus = 0;
            return false;
        }
    }
}
