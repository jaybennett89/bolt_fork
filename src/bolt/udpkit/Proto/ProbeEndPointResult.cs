using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class ProbeEndPointResult : Result {
    public UdpEndPoint WanEndPoint;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref WanEndPoint);
    }
  }
}
