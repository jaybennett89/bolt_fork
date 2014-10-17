using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  class PropertySerializerVector : PropertySerializerSimple {
    PropertySmoothingSettings SmoothingSettings;

    public void AddSettings(PropertySmoothingSettings smoothingSettings) {
      SmoothingSettings = smoothingSettings;
    }

    public override object GetDebugValue(State state) {
      var v = Blit.ReadVector3(state.Frames.first.Data, Settings.ByteOffset);
      return string.Format("X:{0} Y:{1} Z:{2}", v.x.ToString("F3"), v.y.ToString("F3"), v.z.ToString("F3"));
    }

    public override int StateBits(State state, State.Frame frame) {
      return 32 * 3;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      stream.WriteVector3(Blit.ReadVector3(data, offset));
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Blit.PackVector3(data, offset, stream.ReadVector3());
    }

    public override void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) {
      var v0 = from.ReadVector3(Settings.ByteOffset);
      var v1 = to.ReadVector3(Settings.ByteOffset);
      into.PackVector3(Settings.ByteOffset, UE.Vector3.Lerp(v0, v1, t));
    }
  }
}
