using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class Error : Message {
    public string Text;

    protected override void OnSerialize() {
      base.OnSerialize();
      Serialize(ref Text);
    }
  }
}
