using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerString : PropertySerializer {
    public PropertySerializerString(StatePropertyMetaData info)
      : base(info) {
    }

    public PropertySerializerString(EventPropertyMetaData meta)
      : base(meta) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 + (Blit.ReadI32(frame.Data, StateData.ByteOffset) * 8);
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteInt(Blit.ReadI32(frame.Data, StateData.ByteOffset));
      stream.WriteByteArray(frame.Data, StateData.ByteOffset + 4, Blit.ReadI32(frame.Data, StateData.ByteOffset));
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(frame.Data, StateData.ByteOffset, stream.ReadInt());
      Blit.PackBytes(frame.Data, StateData.ByteOffset + 4, stream.ReadByteArray(Blit.ReadI32(frame.Data, StateData.ByteOffset)));
    }
  }
}
