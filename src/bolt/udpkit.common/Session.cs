using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public enum UdpSessionSource {
    Lan,
    Steam,
    Zeus,
  }

  public class UdpSession {
    internal uint _lastSeen;

    internal Guid _id;
    internal UdpEndPoint _wanEndPoint;
    internal UdpEndPoint _lanEndPoint;
    internal UdpSessionSource _source;

    internal string _hostName;
    internal byte[] _hostData;
    internal object _hostObject;

    public Guid Id { get { return _id; } }
    public UdpSessionSource Source { get { return _source; } }
    public UdpEndPoint WanEndPoint { get { return _wanEndPoint; } }
    public UdpEndPoint LanEndPoint { get { return _lanEndPoint; } }

    public string HostName {
      get { return _hostName; }
    }

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
