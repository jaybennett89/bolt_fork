using System;
using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public static class Math {

    internal static float InterpolateFloat(BoltDoubleList<NetworkStorage> frames, int offset, int frame) {
      var f0 = frames.first;
      var p0 = f0.Values[offset].Float1;

      if ((frames.count == 1) || (f0.Frame >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Values[offset].Float1;

        Assert.True(f1.Frame > f0.Frame);
        Assert.True(f1.Frame > frame);

        int f0Frame = f0.Frame;
        if (f0Frame < (f1.Frame - BoltCore.remoteSendRate * 2)) {
          f0Frame = f1.Frame - BoltCore.remoteSendRate * 2;
        }

        float t = f1.Frame - f0Frame;
        float d = frame - f0Frame;

        return UE.Mathf.Lerp(p0, p1, d / t);
      }
    }

    internal static UE.Vector3 InterpolateVector(BoltDoubleList<NetworkStorage> frames, int offset, int frame, float snapLimit) {
      bool snapped = false;
      return InterpolateVector(frames, offset, frame, snapLimit, ref snapped);
    }

    internal static UE.Vector3 InterpolateVector(BoltDoubleList<NetworkStorage> frames, int offset, int frame, float snapLimit, ref bool snapped) {
      var f0 = frames.first;
      var p0 = f0.Values[offset].Vector3;

      if ((frames.count == 1) || (f0.Frame >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Values[offset].Vector3;

        Assert.True(f1.Frame > f0.Frame);
        Assert.True(f1.Frame > frame);

        if ((p0 - p1).sqrMagnitude > (snapLimit * snapLimit)) {
          snapped = true;
          return p1;
        }
        else {
          int f0Frame = f0.Frame;

          if (f0Frame < (f1.Frame - BoltCore.remoteSendRate * 2)) {
            f0Frame = f1.Frame - BoltCore.remoteSendRate * 2;
          }

          float t = f1.Frame - f0Frame;
          float d = frame - f0Frame;

          return UE.Vector3.Lerp(p0, p1, d / t);
        }
      }
    }

    internal static UE.Quaternion InterpolateQuaternion(BoltDoubleList<NetworkStorage> frames, int offset, int frame) {
      var f0 = frames.first;
      var p0 = f0.Values[offset].Quaternion;
      if (p0 == default(UE.Quaternion)) {
        p0 = UE.Quaternion.identity;
      }

      if ((frames.count == 1) || (f0.Frame >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Values[offset].Quaternion;
        if (p1 == default(UE.Quaternion)) {
          p1 = UE.Quaternion.identity;
        }

        Assert.True(f1.Frame > f0.Frame);
        Assert.True(f1.Frame > frame);

        int f0Frame = f0.Frame;
        if (f0Frame < (f1.Frame - BoltCore.remoteSendRate * 2)) {
          f0Frame = f1.Frame - BoltCore.remoteSendRate * 2;
        }

        float t = f1.Frame - f0Frame;
        float d = frame - f0Frame;

        return UE.Quaternion.Lerp(p0, p1, d / t);
      }
    }

    internal static UE.Vector3 ExtrapolateVector(BoltDoubleList<NetworkStorage> frames, int offset, int frame, PropertySmoothingSettings settings, ref bool snapped) {
      throw new NotImplementedException();
      //var f = frames.first;
      //var tolerance = settings.ExtrapolationErrorTolerance;

      //UE.Vector3 p = f.Data.ReadVector3(offset);
      //UE.Vector3 v = velocity * BoltNetwork.frameDeltaTime;

      //float d = System.Math.Min(settings.ExtrapolationMaxFrames, (frame + 1) - f.Number);
      //float t = d / System.Math.Max(1, settings.ExtrapolationCorrectionFrames);

      //UE.Vector3 p0 = position + v;
      //UE.Vector3 p1 = p + (v * d);

      //if ((p1 - p0).sqrMagnitude > (settings.SnapMagnitude * settings.SnapMagnitude)) {
      //  snapped = true;
      //  return p1;
      //}
      //else {
      //  //if (velocity.magnitude < 0.1f && ((p1 - p0).magnitude < tolerance)) {
      //  //  return position;
      //  //}

      //  return UE.Vector3.Lerp(p0, p1, t);
      //}
    }

    internal static UE.Quaternion ExtrapolateQuaternion(BoltDoubleList<NetworkStorage> frames, int offset, int frame, PropertySmoothingSettings settings) {
      throw new NotImplementedException();

      //var r0 = rotation;
      //if (r0 == default(UE.Quaternion)) {
      //  r0 = UE.Quaternion.identity;
      //}

      //var r1 = frames.first.Data.ReadQuaternion(offset);
      //if (r1 == default(UE.Quaternion)) {
      //  r1 = UE.Quaternion.identity;
      //}

      //var r2 = r1 * UE.Quaternion.Inverse(r0);
      //float d = System.Math.Min(settings.ExtrapolationMaxFrames, (frame + 1) - frames.first.Number);
      //float t = d / System.Math.Max(2, settings.ExtrapolationCorrectionFrames);

      //float r2_angle;
      //UE.Vector3 r2_axis;

      //r2.ToAngleAxis(out r2_angle, out r2_axis);

      //if (r2_angle > 180) {
      //  r2_angle -= 360;
      //}

      //r2_angle = (r2_angle * t) % 360f;

      //return UE.Quaternion.AngleAxis(r2_angle, r2_axis) * r0;
    }

    internal static int SequenceDistance(uint from, uint to, int shift) {
      from <<= shift;
      to <<= shift;
      return ((int)(from - to)) >> shift;
    }

    public static int HighBit(uint v) {
      int bit = 0;

      while (v > 0) {
        bit += 1;
        v >>= 1;
      }

      return bit;
    }

    public static int BytesRequired(int bits) {
      return (bits + 7) >> 3;
    }

    public static int BitsRequired(int number) {
      if (number < 0) {
        return 32;
      }

      if (number == 0) {
        return 1;
      }

      for (int i = 31; i >= 0; --i) {
        int b = 1 << i;

        if ((number & b) == b) {
          return i + 1;
        }
      }

      throw new Exception();
    }

    public static int PopCount(ulong value) {
      int count = 0;

      for (int i = 0; i < 32; ++i) {
        if ((value & (1UL << i)) != 0) {
          count += 1;
        }
      }

      return count;
    }
  }
}