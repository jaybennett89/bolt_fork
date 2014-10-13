﻿using UdpKit;

namespace Bolt {
  class PropertySerializerPrefabId : PropertySerializerSimple {
    public PropertySerializerPrefabId(StatePropertyMetaData info) : base(info) { }
    public PropertySerializerPrefabId(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerPrefabId(CommandPropertyMetaData meta) : base(meta) { }

    public override int StateBits(State state, State.Frame frame) {
      return 32;
    }

    public override object GetDebugValue(State state) {
      return Blit.ReadPrefabId(state.Frames.first.Data, StateData.ByteOffset);
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteInt(Blit.ReadPrefabId(data, offset).Value);
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackPrefabId(data, offset, new Bolt.PrefabId(stream.ReadInt()));
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {

    }
  }
}
