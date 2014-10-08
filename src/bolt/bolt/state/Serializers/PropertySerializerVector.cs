using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerVector : PropertySerializer {
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

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteVector3(frame.Data.ReadVector3(StateData.ByteOffset));
#if BOLT_PROPERTY_TRACE
      BoltLog.Debug("W-{0}: {1} - {2} bits", StateData.PropertyName, frame.Data.ReadVector3(StateData.ByteOffset), 4 * 3);
#endif
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackVector3(StateData.ByteOffset, stream.ReadVector3());
#if BOLT_PROPERTY_TRACE
      BoltLog.Debug("R-{0}: {1} - {2} bits", StateData.PropertyName, frame.Data.ReadVector3(StateData.ByteOffset), 4 * 3);
#endif
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteVector3(data.ReadVector3(CommandData.ByteOffset));
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      data.PackVector3(CommandData.ByteOffset, stream.ReadVector3());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadVector3(CommandData.ByteOffset);
      var v1 = to.ReadVector3(CommandData.ByteOffset);
      into.PackVector3(CommandData.ByteOffset, UE.Vector3.Lerp(v0, v1, t));
    }
  }
}
