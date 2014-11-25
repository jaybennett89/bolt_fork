using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Quaternion : NetworkProperty {
    PropertyQuaternionCompression Compression;

    public override bool WantsOnSimulateBefore {
      get { return Interpolation.Enabled; }
    }

    public void Settings_Quaternion(PropertyFloatCompressionSettings compression) {
      Compression = PropertyQuaternionCompression.Create(compression);
    }

    public void Settings_QuaternionEuler(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z) {
      Compression = PropertyQuaternionCompression.Create(PropertyVectorCompressionSettings.Create(x, y, z));
    }

    public override void OnInit(NetworkObj obj) {
      obj.Storage.Values[obj[this]].Quaternion = UE.Quaternion.identity;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].NetworkId;
    }

    public override int BitCount(NetworkObj obj) {
      return Compression.BitsRequired;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      Compression.Pack(packet, storage.Values[obj[this]].Quaternion);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      UE.Quaternion q = Compression.Read(packet);

      if (Interpolation.Enabled) {
        storage.Values[obj[this] + 1].Quaternion = q;
      }
      else {
        storage.Values[obj[this]].Quaternion = q;
      }
    }

    public override void OnSimulateBefore(NetworkObj obj) {
      if (Interpolation.Enabled) {
        obj.Storage.Values[obj[this]].Quaternion = Math.InterpolateQuaternion(obj.RootState.Frames, obj[this] + 1, obj.RootState.Entity.Frame);
      }
    }
  }
}
