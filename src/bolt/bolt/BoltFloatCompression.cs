using UnityEngine;

internal static class BoltFloatCompression {
  public static byte ByteNegOneOne (float value) {
    return (byte)((Mathf.Clamp(value, -1f, 1f) + 1f) * 127f);
  }

  public static float ByteNegOneOne (byte value) {
    return Mathf.Clamp((value * (1f / 127f)) - 1f, -1f, 1f);
  }

  public static byte ByteZeroOne (float value) {
    return (byte)(Mathf.Clamp01(value) * 255f);
  }

  public static float ByteZeroOne (byte value) {
    return Mathf.Clamp01(value * (1f / 255f));
  }

  public static byte ByteAngle (float value) {
    return (byte)(Mathf.Clamp(value, 0, 360) * 0.708f);
  }

  public static float ByteAngle (byte value) {
    return value * 1.412f;
  }

  public static byte ByteAngle180 (float value) {
    return ByteAngle(Mathf.Clamp(value, -180f, 180f) + 180f);
  }

  public static float ByteAngle180 (byte value) {
    return Mathf.Clamp(ByteAngle(value) - 180f, -180f, 180f);
  }
}
