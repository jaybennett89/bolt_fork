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
      base.OnSerialize();

      Create(ref Host);

      Serialize(ref Host.HostName);
      Serialize(ref Host.HostData);
      Serialize(ref Host.LanEndPoint);
      Serialize(ref Host.UPnP_Result);
      Serialize(ref Host.NatProbe_Result);
    }
  }
}
