using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerVector : PropertySerializerSimple {
    public PropertySerializerVector(StatePropertyMetaData info)
      : base(info) {
    }

    public PropertySerializerVector(EventPropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerVector(CommandPropertyMetaData meta)
      : base(meta) {
    }

    public override void DisplayDebugValue(State state) {
      BoltGUI.Label(Blit.ReadVector3(state.Frames.first.Data, StateData.ByteOffset));
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 8;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteVector3(Blit.ReadVector3(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackVector3(data, offset, stream.ReadVector3());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadVector3(CommandData.ByteOffset);
      var v1 = to.ReadVector3(CommandData.ByteOffset);
      into.PackVector3(CommandData.ByteOffset, UE.Vector3.Lerp(v0, v1, t));
    }
  }
}
