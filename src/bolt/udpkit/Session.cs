using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public enum UdpConnectivityStatus {
    Unknown,
    DirectConnection,
    RequiresIntroduction,
    ReverseDirectConnection,
  }

  public enum UdpConnectMode {
    NotPossible,
    DirectConnection,
    ReverseConnection,
    IntroduceConnection
  }

  public class UdpSession {
    internal uint LastSeen;

    internal NAT.UPnP.Result UPnP_Result;
    internal NAT.Probe.Result NatProbe_Result;

    internal UdpConnectivityStatus ConnectivityStatus {
      get {
        if (UPnP_Result == NAT.UPnP.Result.PortOpened) {
          return UdpConnectivityStatus.DirectConnection;
        }

        if ((NatProbe_Result & NAT.Probe.Result.AllowsUnsolicitedTraffic) == NAT.Probe.Result.AllowsUnsolicitedTraffic) {
          return UdpConnectivityStatus.RequiresIntroduction;
        }

        if ((NatProbe_Result & NAT.Probe.Result.EndPointPreservation) == NAT.Probe.Result.EndPointPreservation) {
          return UdpConnectivityStatus.RequiresIntroduction;
        }

        if ((UPnP_Result == NAT.UPnP.Result.Failed) && (NatProbe_Result == NAT.Probe.Result.Failed)) {
          return UdpConnectivityStatus.ReverseDirectConnection;
        }

        return UdpConnectivityStatus.Unknown;
      }
    }

    public Guid Id;
    public UdpEndPoint WanEndPoint;
    public UdpEndPoint LanEndPoint;

    public string HostName;
    public byte[] HostData;

    public bool IsWan {
      get { return (WanEndPoint != UdpEndPoint.Any) && (WanEndPoint.Address.IsPrivate == false); }
    }

    public bool IsLan {
      get { return (WanEndPoint == UdpEndPoint.Any) && (LanEndPoint != UdpEndPoint.Any) && (LanEndPoint.Address.IsPrivate == true); }
    }

    public UdpSession() {

    }
  }
}
