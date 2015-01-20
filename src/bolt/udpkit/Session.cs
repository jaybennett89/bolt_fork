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

  public enum UdpSessionSource {
    Lan,
    Steam,
    MasterServer,
  }

  public enum UdpSessionType {
    DedicatedServer,
    PlayerHost
  }

  public class UdpSession {
    internal uint _lastSeen;
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


    internal Guid _id;
    internal UdpEndPoint _wanEndPoint;
    internal UdpEndPoint _lanEndPoint;
    internal UdpSessionSource _source;

    internal string _hostName;
    internal byte[] _hostData;

    public Guid Id { get { return _id; } }
    public UdpSessionSource Source { get { return _source; } }
    public UdpEndPoint WanEndPoint { get { return _wanEndPoint; } }
    public UdpEndPoint LanEndPoint { get { return _lanEndPoint; } }

    public string HostName { get { return _hostName; } }
    public byte[] HostData { get { return _hostData; } }

    public bool HasWan {
      get { return WanEndPoint.IsWan; }
    }

    public bool HasLan {
      get { return LanEndPoint.IsLan; }
    }

    public UdpSession() {

    }

    internal UdpSession Clone() {
      return (UdpSession)MemberwiseClone();
    }
  }
}
