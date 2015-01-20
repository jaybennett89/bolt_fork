using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_Session_Info : Message {
    public UdpSession Host;

    protected override void OnSerialize() {
      Serialize(ref Host);
    }
  }
}
