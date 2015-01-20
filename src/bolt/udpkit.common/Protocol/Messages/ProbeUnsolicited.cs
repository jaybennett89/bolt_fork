using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class ProbeUnsolicited : Message {
    public UdpEndPoint WanEndPoint;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref WanEndPoint);
    }
  }
}
