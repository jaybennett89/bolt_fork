using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_Session_Info : Message {
    public UdpSession Host;

    protected override void OnSerialize() {
      base.OnSerialize();

      if (!Pack) {
        Host = new UdpSession();
      }

      Serialize(ref Host.Id);
      Serialize(ref Host.HostName);
      Serialize(ref Host.HostData);
      Serialize(ref Host.WanEndPoint);
      Serialize(ref Host.LanEndPoint);
    }
  }
}
