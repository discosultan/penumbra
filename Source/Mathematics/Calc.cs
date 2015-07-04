using System;

namespace Penumbra.Mathematics
{
    internal static class Calc
    {
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

        public static int Step(float y, float x)
        {
            return x >= y ? 1 : 0;
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
