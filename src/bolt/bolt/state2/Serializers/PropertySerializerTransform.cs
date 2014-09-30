using System;

namespace Bolt {
  internal class PropertySerializerTransform : PropertySerializer {
    public PropertySerializerTransform(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(byte[] data) {
      return 12 + 16;
    }

    public override void OnSimulateBefore(State state) {
      if (!state.Entity.IsOwner && !state.Entity.HasControl) {

      }
    }

    public override void OnSimulateAfter(State state) {
      if (state.Entity.IsOwner) {

      }
    }

    public override void Pack(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
    }

    public override void Read(State.Frame frame, BoltConnection connection, UdpKit.UdpStream stream) {
    }
  }
}
