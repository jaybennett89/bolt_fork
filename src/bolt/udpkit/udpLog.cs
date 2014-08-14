using System;
using System.Diagnostics;
using System.Threading;

namespace UdpKit {
    public static class UdpLog {
        public delegate void Writer (uint level, string message);

        public const uint ERROR = 0;
        public const uint INFO = 1;
        public const uint USER = 2;
        public const uint DEBUG = 4;
        public const uint TRACE = 8;
        public const uint WARN = 16;

        static uint enabled = INFO | USER | DEBUG | TRACE | WARN | ERROR;
        static Writer writer = null;
        static readonly object sync = new object();

        static void Write (uint level, string message) {
            lock (sync) {
                Writer callback = writer;

                if (callback != null)
                    callback(level, message);
            }
        }

        static string Name () {
			return "udpkit";
        }

        static public void Info (string format, params object[] args) {
            if (UdpMath.IsSet(enabled, INFO))
                Write(INFO, String.Concat(Name(), " | info  | ", String.Format(format, args)));
        }

        static public void User (string format, params object[] args) {
            if (UdpMath.IsSet(enabled, INFO))
                Write(USER, String.Concat(Name(), " | user  | ", String.Format(format, args)));
        }

        [Conditional("TRACE")]
        static public void Trace (string format, params object[] args) {
#if TRACE
            if (UdpMath.IsSet(enabled, TRACE))
				Write(TRACE, String.Concat(Name(), " | trace | ", String.Format(format, args)));
#endif
        }

        [Conditional("DEBUG")]
        static public void Debug (string format, params object[] args) {
#if DEBUG
            if (UdpMath.IsSet(enabled, DEBUG))
                Write(DEBUG, String.Concat(Name(), " | debug | ", String.Format(format, args)));
#endif
        }

        static public void Warn (string format, params object[] args) {
            if (UdpMath.IsSet(enabled, WARN)) {
#if DEBUG
                Write(WARN, String.Concat(Name(), " | warn  | ", String.Format(format, args), "\r\n", Environment.StackTrace));
#else
				Write(WARN, String.Concat(Name(), " | warn  | ", String.Format(format, args)));
#endif
            }
        }

        static public void Error (string format, params object[] args) {
#if DEBUG
            Write(ERROR, String.Concat(Name(), " | error | ", String.Format(format, args), "\r\n", Environment.StackTrace));
#else
			Write(ERROR, String.Concat(Name(), " | error | ", String.Format(format, args)));
#endif
        }

        static public void SetWriter (UdpLog.Writer callback) {
            writer = callback;
        }

        static public void Disable (uint flag) {
            enabled &= ~flag;
        }

        static public void Enable (uint flag) {
            enabled |= flag;
        }

        static public bool IsEnabled (uint flag) {
            return (enabled & flag) == flag;
        }
    }
}