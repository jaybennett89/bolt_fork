using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public struct UdpSession {
    internal uint LastUpdate;

    public string Source;
    public Guid ServerId;
    public UdpEndPoint EndPoint;

    public string UserData;
    public string ServerName;

    public byte PlayersCurrent;
    public byte PlayersMax;
  }
}
