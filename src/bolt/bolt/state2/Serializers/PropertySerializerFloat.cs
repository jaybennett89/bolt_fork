using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerFloat : PropertySerializer {
    public PropertySerializerFloat(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(State state, State.Frame frame) {
      return 32;
    }

    public override bool Pack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(frame.Data, MetaData.ByteOffset));
      return true;
    }

    public override void Read(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackF32(frame.Data, MetaData.ByteOffset, stream.ReadFloat());
    }

  }
}
