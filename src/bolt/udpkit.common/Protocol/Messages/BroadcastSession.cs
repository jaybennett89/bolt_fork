using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class BroadcastSession : Message {
    public UdpSession Host;

    protected override void OnSerialize() {
      Serialize(ref Host);
    }
  }
}
