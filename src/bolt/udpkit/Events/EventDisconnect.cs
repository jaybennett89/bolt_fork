using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventDisconnect : UdpEventBase {
    public byte[] Token;
    public UdpConnection Connection;

    public override int Type {
      get { return UdpEvent.INTERNAL_DISCONNECT; }
    }
  }

  class UdpEventDisconnected : UdpEventBase {
    public UdpConnection Connection;

    public override int Type {
      get { return UdpEvent.PUBLIC_DISCONNECTED; }
    }
  }
}
