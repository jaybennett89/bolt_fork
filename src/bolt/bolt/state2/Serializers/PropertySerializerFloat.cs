using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UE = UnityEngine;

namespace Bolt {

  class PropertySerializerFloat : PropertySerializerMecanim {
    public PropertySerializerFloat(StatePropertyMetaData info)
      : base(info) {
    }

    public PropertySerializerFloat(EventPropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerFloat(CommandPropertyMetaData meta)
      : base(meta) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override void SetDynamic(State state, object value) {
      state.Frames.first.Data.PackF32(CommandData.ByteOffset, (float)value);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackF32(StateData.ByteOffset, state.Animator.GetFloat(StateData.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetFloat(StateData.PropertyName, Blit.ReadF32(state.Frames.first.Data, StateData.ByteOffset), MecanimData.Damping, BoltCore.frameDeltaTime);
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(frame.Data, StateData.ByteOffset));
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      Blit.PackF32(frame.Data, StateData.ByteOffset, stream.ReadFloat());
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteFloat(data.ReadF32(CommandData.ByteOffset));
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      data.PackF32(CommandData.ByteOffset, stream.ReadFloat());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      float v0 = from.ReadF32(CommandData.ByteOffset);
      float v1 = to.ReadF32(CommandData.ByteOffset);
      into.PackF32(CommandData.ByteOffset, UE.Mathf.Lerp(v0, v1, t));
    }
  }
}
