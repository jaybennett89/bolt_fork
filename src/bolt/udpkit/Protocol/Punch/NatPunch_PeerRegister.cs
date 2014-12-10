using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class NatPunch_PeerRegister : Query<Ack> {
    public Guid Remote;
    public UdpEndPoint Lan;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Remote);
      Serialize(ref Lan);
    }
  }
}
