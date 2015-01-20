using System;
using System.Text;
using System.Collections.Generic;

namespace UdpKit {
  public abstract class UdpPlatform {
    public object Token { get; set; }

    public abstract bool SupportsBroadcast { get; }
    public abstract bool SupportsMasterServer { get; }
    public abstract uint GetPrecisionTime();

    public abstract UdpIPv4Address GetBroadcastAddress();
    public abstract UdpIPv4Address[] ResolveHostAddresses(string host);
    public abstract UdpPlatformSocket CreateSocket();
    public abstract List<UdpPlatformInterface> GetNetworkInterfaces();

    public UdpPlatformSocket CreateSocket(UdpEndPoint endpoint) {
      UdpPlatformSocket socket;

      socket = CreateSocket();
      socket.Bind(endpoint);

      return socket;
    }
  }
}
