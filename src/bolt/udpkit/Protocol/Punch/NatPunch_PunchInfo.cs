using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class NatPunch_PunchInfo : Query<Ack> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }

    public uint Ping;
    public UdpEndPoint PunchTo;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Ping);
      Serialize(ref PunchTo);
    }
  }
}
