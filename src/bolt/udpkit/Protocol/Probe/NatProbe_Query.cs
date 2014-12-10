using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class NatProbe_TestEndPoint : Query<NatProbe_TestEndPoint_Result> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }
  }
}
