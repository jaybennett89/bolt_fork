using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  class PropertySerializerBool : PropertySerializerMecanim {
    public PropertySerializerBool(StatePropertyMetaData info)
      : base(info) {
    }

    public PropertySerializerBool(EventPropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerBool(CommandPropertyMetaData meta)
      : base(meta) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 1;
    }

    public override void SetDynamic(State state, object value) {
      state.Frames.first.Data.PackI32(CommandData.ByteOffset, ((bool)value) ? 1 : 0);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackI32(StateData.ByteOffset, state.Animator.GetBool(StateData.PropertyName) ? 1 : 0);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(StateData.PropertyName, state.Frames.first.Data.ReadI32(StateData.ByteOffset) != 0);
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(frame.Data.ReadI32(StateData.ByteOffset) != 0);
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackI32(CommandData.ByteOffset, stream.ReadBool() ? 1 : 0);
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(data.ReadI32(CommandData.ByteOffset) != 0);
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      data.PackI32(CommandData.ByteOffset, stream.ReadBool() ? 1 : 0);
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
