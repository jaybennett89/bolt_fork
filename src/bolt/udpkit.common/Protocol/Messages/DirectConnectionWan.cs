using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class DirectConnectionWan : Message {
    public Guid RemotePeerId;
    public UdpEndPoint RemoteEndPoint;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref RemotePeerId);
      Serialize(ref RemoteEndPoint);
    }
  }
}
