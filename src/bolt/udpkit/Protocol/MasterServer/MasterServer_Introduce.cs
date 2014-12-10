using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class MasterServer_Introduce : Query<MasterServer_Introduce_Result> {
    public override bool IsUnique {
      get { return true; }
    }

    public override bool Resend {
      get { return true; }
    }

    public UdpSession Host;
    public UdpSession Client;

    protected override void OnSerialize() {
      base.OnSerialize();

      Create(ref Host);
      Serialize(ref Host.Id);

      Create(ref Client);
      Serialize(ref Client.Id);
      Serialize(ref Client.LanEndPoint);
      Serialize(ref Client.UPnP_Result);
      Serialize(ref Client.NatProbe_Result);
    }
  }

  enum MasterServer_Introduce_Result_Status {
    HostGone,
    CantConnect,
    DirectConnection,
    IntroductionRequired
  }

  class MasterServer_Introduce_Result : Result {
    public UdpEndPoint IntroduceEndPoint;
    public MasterServer_Introduce_Result_Status Status;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Status);
      Serialize(ref IntroduceEndPoint);
    }
  }
}
