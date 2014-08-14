using System;
using System.Diagnostics;
using System.Threading;

namespace UdpKit {
    static class UdpAssert {
        [Conditional("DEBUG")]
        internal static void Assert (bool condition) {
            if (!condition)
                throw new UdpException("assert failed");
        }

        [Conditional("DEBUG")]
        internal static void Assert (bool condition, string message) {
            if (!condition)
                throw new UdpException(String.Concat("assert failed: ", message));
        }

        [Conditional("DEBUG")]
        internal static void Assert (bool condition, string message, params object[] args) {
            if (!condition)
                throw new UdpException(String.Concat("assert failed: ", String.Format(message, args)));
        }

        [Conditional("DEBUG")]
        internal static void AssertThread (Thread thread) {
            Assert(ReferenceEquals(Thread.CurrentThread, thread), "expected thread to be '{0}', but was '{1}'", thread.Name, Thread.CurrentThread.Name);
        }
    }
}
