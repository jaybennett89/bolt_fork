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

  public class UdpEventConnectRequest : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_REQUEST; }
    }
  }

  public class UdpEventConnectRefused : UdpEventBase {
    public UdpEndPoint EndPoint;
    public byte[] Token;

    public override int Type {
      get { return UdpEvent.INTERNAL_REFUSE; }
    }
  }

  public class UdpEventConnectFailed : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_FAILED; }
    }
  }

  public class UdpEventConnectAttempt : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.PUBLIC_CONNECT_ATTEMPT; }
    }
  }

  public class UdpEventConnectEndPointCancel : UdpEventBase {
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_CONNECT_CANCEL; }
    }
  }
}
