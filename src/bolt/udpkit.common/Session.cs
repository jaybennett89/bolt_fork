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
