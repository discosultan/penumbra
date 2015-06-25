using System;
using System.Diagnostics;

namespace Penumbra.Utilities
{
    internal static class Check
    {
        [DebuggerHidden]
        public static void ArgumentNotNull(object argument, string argumentName, string message = "")
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName, message);
        }

        [DebuggerHidden]
        public static void True(bool expression, string message = "")
        {
            if (!expression)
                throw new InvalidOperationException(message);
        }

        [DebuggerHidden]
        public static void False(bool expression, string message = "")
        {
            if (expression)
                throw new InvalidOperationException(message);
        }

        [DebuggerHidden]
        public static void ArgumentNotLessThan<T>(T argument, T treshold, string argumentName, string message = "") where T : IComparable<T>
        {
            if (argument.CompareTo(treshold) < 0)
                throw new ArgumentOutOfRangeException(argumentName, message);
        }

        [DebuggerHidden]
        public static void ArgumentNotMoreThan<T>(T argument, T treshold, string argumentName, string message = "") where T : IComparable<T>
        {
            if (argument.CompareTo(treshold) > 0)
                throw new ArgumentOutOfRangeException(argumentName, message);
        }

        [DebuggerHidden]
        public static void ArgumentWithinRange<T>(T argument, T min, T max, string argumentName, string message = "") where T : IComparable<T>
        {
            if ((argument.CompareTo(min) < 0) || (argument.CompareTo(max) > 0))
                throw new ArgumentOutOfRangeException(argumentName, message);
        }
    }
}
