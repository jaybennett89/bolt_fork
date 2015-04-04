using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventSessionConnect : UdpEventBase {
    public byte[] Token;
    public UdpSession Session;

    public override int Type {
      get { return UdpEvent.INTERNAL_SESSION_CONNECT; }
    }
  }

  class UdpEventSessionSetHostData : UdpEventBase {
    public string Name;
    public byte[] Token;
    public bool Dedicated;

    public override int Type {
      get { return UdpEvent.INTERNAL_SESSION_HOST_SETINFO; }
    }
  }

  class UdpEventSessionConnectFailed : UdpEventBase {
    public byte[] Token;
    public UdpSession Session;

    public override int Type {
      get { return UdpEvent.PUBLIC_SESSION_CONNECTFAILED; }
    }
  }

  class UdpEventSessionListUpdated : UdpEventBase {
    public Map<Guid, UdpSession> SessionList;

    public override int Type {
      get { return UdpEvent.PUBLIC_SESSION_LISTUPDATED; }
    }
  }
}
