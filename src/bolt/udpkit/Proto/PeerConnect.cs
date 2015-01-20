using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  sealed class PeerConnect : Query<PeerConnectResult> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }
  }

  sealed class PeerConnectResult : Result {
    public UdpEndPoint WanEndPoint;

    protected override void OnSerialize() {
      Serialize(ref WanEndPoint);
    }
  }
}
