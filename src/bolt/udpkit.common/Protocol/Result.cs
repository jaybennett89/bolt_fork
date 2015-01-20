using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  abstract class Result : Message {
    public Guid Query;

    protected override void OnSerialize() {
      base.OnSerialize();

      Serialize(ref Query);
    }
  }

  sealed class Ack : Result {

  }
}
