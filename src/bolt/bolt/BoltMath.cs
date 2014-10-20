using System;
using UE = UnityEngine;

namespace Bolt {
  [Documentation]
  public static class Math {

    internal static float InterpolateFloat(BoltDoubleList<State.Frame> frames, int offset, int frame) {
      var f0 = frames.first;
      var p0 = f0.Data.ReadF32(offset);

      if ((frames.count == 1) || (f0.Number >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Data.ReadF32(offset);

        Assert.True(f1.Number > f0.Number);
        Assert.True(f1.Number > frame);

        int f0Frame = f0.Number;
        if (f0Frame < (f1.Number - BoltCore.remoteSendRate * 2)) {
          f0Frame = f1.Number - BoltCore.remoteSendRate * 2;
        }

        float t = f1.Number - f0Frame;
        float d = frame - f0Frame;

        return UE.Mathf.Lerp(p0, p1, d / t);
      }
    }

    internal static UE.Vector3 InterpolateVector(BoltDoubleList<State.Frame> frames, int offset, int frame) {
      var f0 = frames.first;
      var p0 = f0.Data.ReadVector3(offset);

      if ((frames.count == 1) || (f0.Number >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Data.ReadVector3(offset);

        Assert.True(f1.Number > f0.Number);
        Assert.True(f1.Number > frame);

        int f0Frame = f0.Number;
        if (f0Frame < (f1.Number - BoltCore.remoteSendRate * 2)) {
          f0Frame = f1.Number - BoltCore.remoteSendRate * 2;
        }

        float t = f1.Number - f0Frame;
        float d = frame - f0Frame;

        return UE.Vector3.Lerp(p0, p1, d / t);
      }
    }

    internal static UE.Quaternion InterpolateQuaternion(BoltDoubleList<State.Frame> frames, int offset, int frame) {
      var f0 = frames.first;
      var p0 = f0.Data.ReadQuaternion(offset);
      if (p0 == default(UE.Quaternion)) {
        p0 = UE.Quaternion.identity;
      }

      if ((frames.count == 1) || (f0.Number >= frame)) {
        return p0;
      }
      else {
        var f1 = frames.Next(f0);
        var p1 = f1.Data.ReadQuaternion(offset);
        if (p1 == default(UE.Quaternion)) {
          p1 = UE.Quaternion.identity;
        }

        Assert.True(f1.Number > f0.Number);
        Assert.True(f1.Number > frame);

        int f0Frame = f0.Number;
        if (f0Frame < (f1.Number - BoltCore.remoteSendRate * 2)) {
          f0Frame = f1.Number - BoltCore.remoteSendRate * 2;
        }

        float t = f1.Number - f0Frame;
        float d = frame - f0Frame;

        return UE.Quaternion.Lerp(p0, p1, d / t);
      }
    }

    internal static float ExtrapolateFloat(BoltDoubleList<State.Frame> frames, int offset, int frame, PropertySmoothingSettings settings, float value) {
      var f = frames.first;

      frame = UE.Mathf.Min(frame, f.Number + settings.ExtrapolationMaxFrames);

      var v0 = value;
      var v1 = f.Data.ReadF32(offset);

      float d = System.Math.Min(settings.ExtrapolationMaxFrames, (frame + 1) - f.Number);
      float t = d / System.Math.Max(1, settings.ExtrapolationCorrectionFrames);

      return v0 + ((v1 - v0) * t);
    }

    internal static UE.Vector3 ExtrapolateVector(BoltDoubleList<State.Frame> frames, int offset, int velocityOffset, int frame, PropertySmoothingSettings settings, UE.Vector3 position) {
      return ExtrapolateVector(frames, offset, frame, settings, position, frames.first.Data.ReadVector3(velocityOffset));
    }

    internal static UE.Vector3 ExtrapolateVector(BoltDoubleList<State.Frame> frames, int offset, int frame, PropertySmoothingSettings settings, UE.Vector3 position, UE.Vector3 velocity) {
      var f = frames.first;
      var tolerance = settings.ExtrapolationErrorTolerance;

      UE.Vector3 p = f.Data.ReadVector3(offset);
      UE.Vector3 m = p - position;
      UE.Vector3 v = velocity * BoltNetwork.frameDeltaTime;

      float d = System.Math.Min(settings.ExtrapolationMaxFrames, (frame + 1) - f.Number);
      float t = d / System.Math.Max(2, settings.ExtrapolationCorrectionFrames);

      p = UE.Vector3.Lerp(position + v, p + (v * d), t);

      if ((velocity.magnitude < tolerance) && ((p - position).magnitude < tolerance)) {
        return position;
      }

      return p;
    }

    internal static UE.Quaternion ExtrapolateQuaternion(BoltDoubleList<State.Frame> frames, int offset, int frame, PropertySmoothingSettings settings, UE.Quaternion rotation) {
      if (rotation == default(UE.Quaternion)) {
        rotation = UE.Quaternion.identity;
      }

      var f = frames.first;
      var r0 = rotation;
      var r1 = f.Data.ReadQuaternion(offset);
      var df = r1 * UE.Quaternion.Inverse(r0);

      float d = System.Math.Min(settings.ExtrapolationMaxFrames, (frame + 1) - f.Number);
      float t = d / System.Math.Max(2, settings.ExtrapolationCorrectionFrames);

      float dAngle;
      UE.Vector3 dAxis;

      df.ToAngleAxis(out dAngle, out dAxis);

      return UE.Quaternion.AngleAxis((dAngle * t) % 360f, dAxis) * r0;
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
  }
}