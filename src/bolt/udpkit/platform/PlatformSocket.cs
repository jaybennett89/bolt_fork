using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit {
  public abstract class UdpPlatformSocket {
    byte[] buffer = new byte[1024];

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

    internal void SendProtocolMessage(UdpEndPoint endpoint, Protocol.Message msg) {
      buffer[0] = Protocol.Message.MESSAGE_HEADER;

      msg.InitBuffer(1, buffer, true);

      UdpLog.Debug("OUT: {0}", msg.GetType());
      SendTo(buffer, msg.Serialize(), endpoint);
    }

    internal Protocol.Message RecvProtocolMessage() {
      UdpEndPoint endpoint = new UdpEndPoint();

      var bytes = RecvFrom(buffer, ref endpoint);

      if (bytes > 0) {
        var offset = 0;

        Protocol.Message msg;

        msg = Platform.ParseMessage(buffer, ref offset);
        msg.Sender = endpoint;

        return msg;
      }

      return null;
    }
  }
}
