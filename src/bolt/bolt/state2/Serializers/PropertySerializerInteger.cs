using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerInteger : PropertySerializerMecanim {
    public PropertySerializerInteger(StatePropertyMetaData info)
      : base(info) {
    }

    public PropertySerializerInteger(EventPropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerInteger(CommandPropertyMetaData meta)
      : base(meta) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override void SetDynamic(State state, object value) {
      state.Frames.first.Data.PackI32(CommandData.ByteOffset, (int)value);
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetInteger(StateData.PropertyName, Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset));
    }

    protected override void PullMecanimValue(State state) {
      Blit.PackI32(state.Frames.first.Data, StateData.ByteOffset, state.Animator.GetInteger(StateData.PropertyName));
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadI32(frame.Data, StateData.ByteOffset));
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackI32(frame.Data, StateData.ByteOffset, stream.ReadInt());
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteInt(data.ReadI32(CommandData.ByteOffset));
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      data.PackI32(CommandData.ByteOffset, stream.ReadInt());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
