using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerColor : PropertySerializerSimple {
    public PropertySerializerColor(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerColor(EventPropertyMetaData meta) : base(meta) { }

    public override object GetDebugValue(State state) {
      return Blit.ReadColor(state.Frames.first.Data, StateData.ByteOffset);
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 3;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteColorRGBA(Blit.ReadColor(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackColor(data, offset, stream.ReadColorRGBA());
    }
  }
}
