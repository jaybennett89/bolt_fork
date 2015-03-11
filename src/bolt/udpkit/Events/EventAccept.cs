using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventAcceptConnect : UdpEventBase {
    public UdpEndPoint EndPoint;
    public byte[] Token;
    public object UserObject;

    public override int Type {
      get { return UdpEvent.INTERNAL_ACCEPT; }
    }
  }

  class UdpEventRefuseConnect : UdpEventBase {
    public UdpEndPoint EndPoint;
    public byte[] Token;

    public override int Type {
      get { return UdpEvent.INTERNAL_REFUSE; }
    }
  }
}
