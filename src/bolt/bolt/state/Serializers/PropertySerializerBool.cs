using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerBool : PropertySerializerMecanim {
    public override int StateBits(State state, State.Frame frame) {
      return 1;
    }

    public override object GetDebugValue(State state) {
      return state.Frames.first.Data.ReadBool(Settings.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackBool(Settings.ByteOffset, state.Animator.GetBool(Settings.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(Settings.PropertyName, state.Frames.first.Data.ReadBool(Settings.ByteOffset));
    }

    protected override bool Pack(byte[] data, BoltConnection connection, UdpStream stream) {
      stream.WriteBool(Blit.ReadBool(data, Settings.ByteOffset));
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpStream stream) {
      Blit.PackBool(data, Settings.ByteOffset, stream.ReadBool());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
