using System;

namespace UdpKit {
    public class UdpException : Exception {
        internal UdpException () : base() { }
        internal UdpException (string msg) : base(msg) { }
        internal UdpException (string fmt, params object[] args) : this(string.Format(fmt, args)) { }
    }
}
