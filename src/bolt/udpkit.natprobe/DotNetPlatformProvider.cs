using System.Collections;
using UdpKit;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;

public class DotNetPlatform : UdpPlatform {
  class PrecisionTimer {
    static readonly long start = Stopwatch.GetTimestamp();
    static readonly double freq = 1.0 / (double)Stopwatch.Frequency;

    internal static uint GetCurrentTime() {
      long diff = Stopwatch.GetTimestamp() - start;
      double seconds = (double)diff * freq;
      return (uint)(seconds * 1000.0);
    }
  }

  public DotNetPlatform() {
    PrecisionTimer.GetCurrentTime();
  }

  public override bool SupportsBroadcast {
    get { return true; }
  }

  public override uint GetPrecisionTime() {
    return PrecisionTimer.GetCurrentTime();
  }

  public override UdpEndPoint FindBroadcastAddress() {
    throw new System.NotImplementedException();
  }

  public override UdpPlatformSocket CreateSocket() {
    return new DotNetSocket();
  }

  public override List<UdpPlatformInterface> GetNetworkInterfaces() {
    return FindInterfaces();
  }

  List<UdpPlatformInterface> FindInterfaces() {
    List<UdpPlatformInterface> result = new List<UdpPlatformInterface>();

    foreach (var n in NetworkInterface.GetAllNetworkInterfaces()) {
      if (n.OperationalStatus != OperationalStatus.Up && n.OperationalStatus != OperationalStatus.Unknown) {
        continue;
      }

      if (n.NetworkInterfaceType == NetworkInterfaceType.Loopback) {
        continue;
      }

      var iface = ParseInterface(n);
      if (iface != null) {
        result.Add(iface);
      }
    }

    return result;
  }


  DotNetInterface ParseInterface(NetworkInterface n) {
    HashSet<UdpIPv4Address> gateway = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);
    HashSet<UdpIPv4Address> unicast = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);
    HashSet<UdpIPv4Address> multicast = new HashSet<UdpIPv4Address>(UdpIPv4Address.Comparer.Instance);

    IPInterfaceProperties p = n.GetIPProperties();

    foreach (var gw in p.GatewayAddresses) {
      if (gw.Address.AddressFamily == AddressFamily.InterNetwork) {
        gateway.Add(ConvertAddress(gw.Address));
      }
    }

    foreach (var addr in p.DnsAddresses) {
      if (addr.AddressFamily == AddressFamily.InterNetwork) {
        gateway.Add(ConvertAddress(addr));
      }
    }

    foreach (var uni in p.UnicastAddresses) {
      if (uni.Address.AddressFamily == AddressFamily.InterNetwork) {
        UdpIPv4Address ipv4 = ConvertAddress(uni.Address);

        unicast.Add(ipv4);
        gateway.Add(new UdpIPv4Address(ipv4.Byte3, ipv4.Byte2, ipv4.Byte1, 1));
      }
    }

    foreach (var multi in p.MulticastAddresses) {
      if (multi.Address.AddressFamily == AddressFamily.InterNetwork) {
        multicast.Add(ConvertAddress(multi.Address));
      }
    }

    if (unicast.Count == 0 || gateway.Count == 0) {
      return null;
    }

    return new DotNetInterface(n, gateway.ToArray(), unicast.ToArray(), multicast.ToArray());
  }


#pragma warning disable 618
  public static UdpEndPoint ConvertEndPoint(EndPoint endpoint) {
    return ConvertEndPoint((IPEndPoint)endpoint);
  }

  public static UdpEndPoint ConvertEndPoint(IPEndPoint endpoint) {
    return new UdpEndPoint(new UdpIPv4Address(endpoint.Address.Address), (ushort)endpoint.Port);
  }

  public static UdpIPv4Address ConvertAddress(IPAddress address) {
    return new UdpIPv4Address(address.Address);
  }

  public static IPEndPoint ConvertEndPoint(UdpEndPoint endpoint) {
    return new IPEndPoint(new IPAddress(new byte[] { endpoint.Address.Byte3, endpoint.Address.Byte2, endpoint.Address.Byte1, endpoint.Address.Byte0 }), endpoint.Port);
  }
#pragma warning restore 618

}
