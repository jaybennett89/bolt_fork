using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UE = UnityEngine;
using UdpKit;

namespace Bolt {
  class PropertySerializerFloat : PropertySerializerMecanim {
    FloatCompression Compression;

    public PropertySerializerFloat(StatePropertyMetaData meta) : base(meta) { }
    public PropertySerializerFloat(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerFloat(CommandPropertyMetaData meta) : base(meta) { }

    public void SetPropertyData(FloatCompression compression) {
      Compression = compression; 
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadF32(state.Frames.first.Data, StateData.ByteOffset);
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackF32(StateData.ByteOffset, state.Animator.GetFloat(StateData.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetFloat(StateData.PropertyName, Blit.ReadF32(state.Frames.first.Data, StateData.ByteOffset), MecanimData.Damping, BoltCore.frameDeltaTime);
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteFloat(Blit.ReadF32(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackF32(data, offset, stream.ReadFloat());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      float v0 = from.ReadF32(CommandData.ByteOffset);
      float v1 = to.ReadF32(CommandData.ByteOffset);
      into.PackF32(CommandData.ByteOffset, UE.Mathf.Lerp(v0, v1, t));
    }
  }
}
