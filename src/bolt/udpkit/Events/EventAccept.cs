using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventAcceptConnect : UdpEventBase {
    public byte[] Token;
    public object UserObject;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_ACCEPT; }
    }
  }

  class UdpEventRefuseConnect : UdpEventBase {
    public byte[] Token;
    public UdpEndPoint EndPoint;

    public override int Type {
      get { return UdpEvent.INTERNAL_REFUSE; }
    }
  }
}
