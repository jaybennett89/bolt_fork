using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventConnectEndPoint : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_CONNECT; }
    }
  }

  class UdpEventConnectRequest : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_REQUEST; }
    }
  }

  class UdpEventConnectRefused : UdpEventBase {
    public UdpEndPoint EndPoint;
    public byte[] Token;

    public override int Type {
      get { return UdpEvent.INTERNAL_REFUSE; }
    }
  }

  class UdpEventConnectFailed : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_FAILED; }
    }
  }

  class UdpEventConnectAttempt : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_ATTEMPT; }
    }
  }

  class UdpEventConnectEndPointCancel : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_CONNECT_CANCEL; }
    }
  }
}
