using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_HostRegister : Query<Ack> {
    public UdpSession Host;

    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }

    protected override void OnSerialize() {
      Serialize(ref Host);
    }
  }
}
