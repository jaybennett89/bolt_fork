using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class NatProbe_TestUnsolicited : Message {
    public int Probe;
    public UdpEndPoint ClientWanEndPoint;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Probe);
      Serialize(ref ClientWanEndPoint);
    }
  }
}
