using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerBool : PropertySerializerMecanim {
    public PropertySerializerBool(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerBool(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerBool(CommandPropertyMetaData meta) : base(meta) { }

    public override int StateBits(State state, State.Frame frame) {
      return 1;
    }

    public override object GetDebugValue(State state) {
      return state.Frames.first.Data.ReadBool(StateData.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackBool(StateData.ByteOffset, state.Animator.GetBool(StateData.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(StateData.PropertyName, state.Frames.first.Data.ReadBool(StateData.ByteOffset));
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteBool(Blit.ReadBool(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackBool(data, offset, stream.ReadBool());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
