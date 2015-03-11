using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventMasterServerConnect : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_MASTERSERVER_CONNECT; }
    }
  }

  class UdpEventMasterServerDisconnect : UdpEventBase {
    public override int Type {
      get { return UdpEvent.INTERNAL_MASTERSERVER_DISCONNECT; }
    }
  }

  class UdpEventMasterServerRequestInfo : UdpEventBase {
    public override int Type {
      get { return UdpEvent.INTERNAL_MASTERSERVER_INFOREQUEST; }
    }
  }

  class UdpEventMasterServerRequestSessionList : UdpEventBase {
    public override int Type {
      get { return UdpEvent.INTERNAL_MASTERSERVER_SESSIONLISTREQUEST; }
    }
  }
}
