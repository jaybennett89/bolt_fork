using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class PeerConnect : Query<PeerConnectResult> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }
  }

  class PeerConnectResult : Result {
    public UdpEndPoint Probe0;
    public UdpEndPoint Probe1;
    public UdpEndPoint Probe2;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Probe0);
      Serialize(ref Probe1);
      Serialize(ref Probe2);
    }
  }
}
