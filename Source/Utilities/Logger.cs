using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Penumbra.Utilities
{
    internal interface ILogger
    {
        void Write(string message, string caller);
    }

    internal class DelegateLogger : ILogger
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public DelegateLogger(Action<string> log)
        {
            Delegate = log ?? throw new ArgumentNullException(nameof(log));
        }

        public Action<string> Delegate { get; }

        public void Write(string message, string caller)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append("[");
            _stringBuilder.Append(DateTime.Now);
            _stringBuilder.Append("][");
            _stringBuilder.Append(caller);
            _stringBuilder.Append("] ");
            _stringBuilder.Append(message);
            Delegate(_stringBuilder.ToString());
        }
    }

    internal static class Logger
    {
        private static readonly List<ILogger> Loggers = new List<ILogger>();

        public static void Write(string message, [CallerMemberName]string caller = "")
        {
            foreach (ILogger logger in Loggers)
                logger.Write(message, caller);
        }

        public static void Add(ILogger logger)
        {
            Loggers.Add(logger);
        }

        public static void Remove(ILogger logger)
        {
            Loggers.Remove(logger);
        }
    }
}
