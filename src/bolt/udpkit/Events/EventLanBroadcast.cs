using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class UdpEventLanBroadcastEnable : UdpEventBase {
    public ushort Port;
    public UdpIPv4Address LocalAddress;
    public UdpIPv4Address BroadcastAddress;

    public override int Type {
      get { return UdpEvent.INTERNAL_LANBROADCAST_ENABLE; }
    }
  }

  class UdpEventLanBroadcastDisable : UdpEventBase {
    public override int Type {
      get { return UdpEvent.INTERNAL_LANBROADCAST_DISABLE; }
    }
  }
}
