using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class GetZeusInfo : Query<GetZeusInfoResult> {
    public override bool Resend {
      get { return true; }
    }

    public override bool IsUnique {
      get { return true; }
    }
  }

  class GetZeusInfoResult : Result {
    public int Hosts;
    public int ClientsInZeus;
    public int ClientsInGames;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Hosts);
      Serialize(ref ClientsInZeus);
      Serialize(ref ClientsInGames);
    }
  }
}
