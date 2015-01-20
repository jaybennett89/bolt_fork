using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class HostInfo : Message {
    public UdpSession Host;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref Host);
    }
  }
}
