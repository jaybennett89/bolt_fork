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

  class UdpEventMasterServerConnected : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_MASTERSERVER_CONNECTED; }
    }
  }

  class UdpEventMasterServerConnectFailed : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_MASTERSERVER_CONNECTFAILED; }
    }
  }

  class UdpEventMasterServerDisconnected : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_MASTERSERVER_DISCONNECTED; }
    }
  }

  class UdpEventMasterServerNatFeatures : UdpEventBase {
    public NatFeatures Features;

    public override int Type {
      get { return UdpEvent.PUBLIC_MASTERSERVER_NATPROBE_RESULT; }
    }
  }

}
