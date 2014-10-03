using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerBool : PropertySerializer {
    public PropertySerializerBool(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(State state, State.Frame frame) {
      return 1;
    }

    public override void OnSimulateAfter(State state) {
      if (state.Animator && MetaData.Mecanim) {
        state.Animator.SetBool(MetaData.PropertyName, state.Frames.first.Data.ReadI32(MetaData.ByteOffset) != 0);
      }
    }

    public override bool Pack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(frame.Data.ReadI32(MetaData.ByteOffset) != 0);
      return true;
    }

    public override void Read(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      if (stream.ReadBool()) {
        Blit.PackI32(frame.Data, MetaData.ByteOffset, 1);
      }
      else {
        Blit.PackI32(frame.Data, MetaData.ByteOffset, 0);
      }
    }
  }
}
