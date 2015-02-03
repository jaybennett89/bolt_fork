using System;
using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public static class Math {
    internal static float InterpolateFloat(BoltDoubleList<NetworkStorage> frames, int offset, int frame, bool angle) {
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

        return angle ? UE.Mathf.LerpAngle(p0, p1, d / t) : UE.Mathf.Lerp(p0, p1, d / t);
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

    internal static UE.Vector3 ExtrapolateVector(UE.Vector3 cpos, UE.Vector3 rpos, UE.Vector3 rvel, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings, ref bool snapped) {
      rvel *= BoltNetwork.frameDeltaTime;

      float d = System.Math.Min(settings.MaxFrames, (entityFrame + 1) - recievedFrame);
      float t = d / System.Math.Max(1, settings.MaxFrames);

      UE.Vector3 p0 = cpos + (rvel);
      UE.Vector3 p1 = rpos + (rvel * d);

      float sqrMag = (p1 - p0).sqrMagnitude;

      if ((settings.SnapMagnitude > 0) && sqrMag > (settings.SnapMagnitude * settings.SnapMagnitude)) {
        snapped = true;
        return p1;
      }
      else {
        //TODO: implement error tolerance
        //if (rvel.sqrMagnitude < sqrMag) {
        //  return p0;
        //}

        return UE.Vector3.Lerp(p0, p1, t);
      }
    }

    internal static UE.Quaternion ExtrapolateQuaternion(UE.Quaternion cquat, UE.Quaternion rquat, int recievedFrame, int entityFrame, PropertyExtrapolationSettings settings) {
      var r = rquat * UE.Quaternion.Inverse(cquat);
      float d = System.Math.Min(settings.MaxFrames, (entityFrame + 1) - recievedFrame);
      float t = d / (float)System.Math.Max(1, settings.MaxFrames);

      float r_angle;
      UE.Vector3 r_axis;

      r.ToAngleAxis(out r_angle, out r_axis);

      if (r_angle > 180) {
        r_angle -= 360;
      }

      return UE.Quaternion.AngleAxis((r_angle * t) % 360f, r_axis) * cquat;
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