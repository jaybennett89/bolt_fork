using System;

namespace UdpKit {
  public class UdpException : Exception {
    internal UdpException() : base() { }
    internal UdpException(string fmt, params object[] args) : this(string.Format(fmt, args)) { }

    internal UdpException(string msg) : base(msg) {
      UdpLog.Error(msg);
      UdpLog.Error(Environment.StackTrace);
    }
  }
}
