﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerColor : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      var c = state.CurrentFrame.Storage[Settings.OffsetStorage].Color;
      return string.Format("R:{0} G:{1} B:{2} A:{3}", c.r.ToString("F3"), c.g.ToString("F3"), c.b.ToString("F3"), c.a.ToString("F3"));
    }

    public override int StateBits(State state, NetworkFrame frame) {
      return 32 * 4;
    }

    protected override bool Pack(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      stream.WriteColorRGBA(storage[Settings.OffsetStorage].Color);
      return true;
    }

    protected override void Read(NetworkValue[] storage, BoltConnection connection, UdpPacket stream) {
      storage[Settings.OffsetStorage].Color = stream.ReadColorRGBA();
    }
  }
}
