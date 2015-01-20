using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public abstract class UdpPlatformSocket {
    public object Token { get; set; }

    public abstract string Error { get; }
    public abstract bool IsBound { get; }
    public abstract bool Broadcast { get; set; }
    public abstract UdpEndPoint EndPoint { get; }
    public abstract UdpPlatform Platform { get; }

    public abstract void Close();
    public abstract void Bind(UdpEndPoint ep);

    public abstract bool RecvPoll();
    public abstract bool RecvPoll(int timeout);

    public abstract int SendTo(byte[] buffer, int bytesToSend, UdpEndPoint endpoint);
    public abstract int RecvFrom(byte[] buffer, int bufferSize, ref UdpEndPoint remoteEndpoint);

    public int SendTo(byte[] buffer, UdpEndPoint endpoint) {
      return SendTo(buffer, buffer.Length, endpoint);
    }

    public int RecvFrom(byte[] buffer, ref UdpEndPoint endpoint) {
      return RecvFrom(buffer, buffer.Length, ref endpoint);
    }
  }
}
