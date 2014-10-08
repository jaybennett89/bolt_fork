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

    public override void DisplayDebugValue(State state) {
      BoltGUI.Label(state.Frames.first.Data.ReadBool(StateData.ByteOffset));
    }

    protected override void PullMecanimValue(State state) {
      state.Frames.first.Data.PackBool(StateData.ByteOffset, state.Animator.GetBool(StateData.PropertyName));
    }

    protected override void PushMecanimValue(State state) {
      state.Animator.SetBool(StateData.PropertyName, state.Frames.first.Data.ReadBool(StateData.ByteOffset));
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(frame.Data.ReadBool(StateData.ByteOffset));

#if BOLT_PROPERTY_TRACE
      BoltLog.Debug("W-{0}: {1} - {2} bits", StateData.PropertyName, frame.Data.ReadI32(StateData.ByteOffset) != 0, 1);
#endif
      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
      frame.Data.PackBool(CommandData.ByteOffset, stream.ReadBool());

#if BOLT_PROPERTY_TRACE
      BoltLog.Debug("R-{0}: {1} - {2} bits", StateData.PropertyName, frame.Data.ReadI32(StateData.ByteOffset) != 0, 1);
#endif
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      stream.WriteBool(data.ReadBool(CommandData.ByteOffset));
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpKit.UdpStream stream) {
      data.PackBool(CommandData.ByteOffset, stream.ReadBool());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
