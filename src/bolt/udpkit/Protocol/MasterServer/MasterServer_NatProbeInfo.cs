using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_NatProbeInfo : Query<MasterServer_NatProbeInfo_Result> {
    public override bool IsUnique {
      get { return true; }
    }

    public override bool Resend {
      get { return true; }
    }
  }

  class MasterServer_NatProbeInfo_Result : Result {
    public UdpEndPoint Probe0;
    public UdpEndPoint Probe1;
    public UdpEndPoint Probe2;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Probe0);
      Serialize(ref Probe1);
      Serialize(ref Probe2);
    }
  }
}
  