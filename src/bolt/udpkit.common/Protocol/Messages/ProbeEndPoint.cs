using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class ProbeEndPoint : Query<ProbeEndPointResult> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }
  }

  class ProbeEndPointResult : Result {
    public UdpEndPoint WanEndPoint;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref WanEndPoint);
    }
  }
}
