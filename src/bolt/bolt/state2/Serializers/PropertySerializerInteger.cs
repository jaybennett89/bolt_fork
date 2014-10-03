using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerInteger : PropertySerializer {
    public PropertySerializerInteger(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(State state, State.Frame frame) {
      return 32;
    }

    public override void OnSimulateAfter(State state) {
      if (state.Animator && MetaData.Mecanim) {
        state.Animator.SetInteger(MetaData.PropertyName, Blit.ReadI32(state.Frames.first.Data, MetaData.ByteOffset));
      }
    }

    public override bool Pack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadI32(frame.Data, MetaData.ByteOffset));
      return true;
    }

    public override void Read(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(frame.Data, MetaData.ByteOffset, stream.ReadInt());
    }
  }
}
