using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_IntroduceInfo : Query<Ack> {
    public Guid Remote;
    public UdpEndPoint PunchServer;

    public override bool Resend {
      get { return true; }
    }

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Remote);
      Serialize(ref PunchServer);
    }
  }
}
