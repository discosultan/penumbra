using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Check.ArgumentNotNull(log, nameof(log));
            Delegate = log;
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
        private static readonly List<ILogger> _loggers = new List<ILogger>();

        [Conditional("DEBUG")]
        public static void Write(object message, [CallerMemberName]string caller = "")
        {
            Write(message.ToString(), caller);
        }

        [Conditional("DEBUG")]
        public static void Write(string message, [CallerMemberName]string caller = "")
        {
            foreach (ILogger logger in _loggers)
            {
                logger.Write(message, caller);
            }
        }

        [Conditional("DEBUG")]
        public static void Add(ILogger logger)
        {
            _loggers.Add(logger);
        }

        [Conditional("DEBUG")]
        public static void Remove(ILogger logger)
        {
            _loggers.Remove(logger);
        }
    }
}
