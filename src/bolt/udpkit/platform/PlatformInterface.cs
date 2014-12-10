using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public enum UdpLinkType {
    Unknown,
    Wifi,
    Ethernet,
    Mobile
  }

  public abstract class UdpPlatformInterface {
    public object Token { get; set; }

    public abstract string Name { get; }
    public abstract byte[] PhysicalAddress { get; }
    public abstract UdpLinkType LinkType { get; }

    public abstract UdpIPv4Address[] UnicastAddresses { get; }
    public abstract UdpIPv4Address[] MulticastAddresses { get; }
    public abstract UdpIPv4Address[] GatewayAddresses { get; }
  }
}
