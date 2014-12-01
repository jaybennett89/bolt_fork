using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Vector : NetworkProperty {
    PropertyVectorCompressionSettings Compression;

    public override bool WantsOnSimulateBefore {
      get { return Interpolation.Enabled; }
    }

    public override object GetDynamic(NetworkObj obj) {
      return obj.Storage.Values[obj[this]].Vector3;
    }

    public override void SetDynamic(NetworkObj obj, object value) {
      var v = (UE.Vector3)value;

      if (NetworkValue.Diff(obj.Storage.Values[obj[this]].Vector3, v)) {
        obj.Storage.Values[obj[this]].Vector3 = v;
        obj.Storage.PropertyChanged(obj.OffsetProperties + this.OffsetProperties);
      }
    }

    public void Settings_Vector(PropertyFloatCompressionSettings x, PropertyFloatCompressionSettings y, PropertyFloatCompressionSettings z) {
      Compression = PropertyVectorCompressionSettings.Create(x, y, z);
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Vector3;
    }

    public override int BitCount(NetworkObj obj) {
      return Compression.BitsRequired;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      Compression.Pack(packet, storage.Values[obj[this]].Vector3);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      UE.Vector3 v = Compression.Read(packet);

      if (Interpolation.Enabled && (obj.Root is NetworkState)) {
        storage.Values[obj[this] + 1].Vector3 = v;
      }
      else {
        storage.Values[obj[this]].Vector3 = v;
      }
    }

    public override void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t) {
      if (Interpolation.Enabled) {
        var v0 = from.Values[obj[this]].Vector3;
        var v1 = to.Values[obj[this]].Vector3;
        var m = (v1 - v0).sqrMagnitude;

        if (m < (Interpolation.SnapMagnitude * Interpolation.SnapMagnitude)) {
          storage.Values[obj[this]].Vector3 = UE.Vector3.Lerp(v0, v1, t);
        }
        else {
          storage.Values[obj[this]].Vector3 = v1;
        }
      }
      else {
        storage.Values[obj[this]].Vector3 = to.Values[obj[this]].Vector3;
      }
    }

    public override void OnSimulateBefore(NetworkObj obj) {
      if (Interpolation.Enabled) {
        obj.Storage.Values[obj[this]].Vector3 = Math.InterpolateVector(obj.RootState.Frames, obj[this] + 1, obj.RootState.Entity.Frame, Interpolation.SnapMagnitude);
      }
    }
  }
}
