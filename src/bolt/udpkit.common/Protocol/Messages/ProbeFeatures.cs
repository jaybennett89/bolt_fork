using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class ProbeFeatures : Query {
    public NatFeatures NatFeatures;

    public override bool IsUnique {
      get { return true; }
    }

    public override bool Resend {
      get { return true; }
    }

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref NatFeatures);
    }
  }
}
