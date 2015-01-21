using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UnityEngine;

[Documentation(Ignore = true)]
public static class BoltUtils {
  public static IProtocolToken GetProtocolToken(this UdpSession session) {
    if (session._hostData == null || session._hostData.Length == 0) {
      return null;
    }

    if (session._hostObject == null) {
      session._hostObject = session._hostData.ToToken();
    }

    return (IProtocolToken)session._hostObject;
  }

  public static bool NullOrEmpty(this Array array) {
    return array == null || array.Length == 0;
  }

  public static bool Has<T> (this T[] array, int index) where T : class {
    return index < array.Length && array[index] != null;
  }

  public static bool Has<T> (this T[] array, uint index) where T : class {
    return index < array.Length && array[index] != null;
  }

  public static bool TryGetIndex<T> (this T[] array, int index, out T value) where T : class {
    if (index < array.Length)
      return (value = array[index]) != null;

    value = default(T);
    return false;
  }

  public static bool TryGetIndex<T> (this T[] array, uint index, out T value) where T : class {
    if (index < array.Length)
      return (value = array[index]) != null;

    value = default(T);
    return false;
  }

  public static T FindComponent<T> (this Component component) where T : Component {
    return FindComponent<T>(component.transform);
  }

  public static T FindComponent<T> (this GameObject gameObject) where T : Component {
    return FindComponent<T>(gameObject.transform);
  }

  public static T FindComponent<T> (this Transform transform) where T : Component {
    T component = null;

    while (transform && !component) {
      component = transform.GetComponent<T>();
      transform = transform.parent;
    }

    return component;
  }

  public static BoltConnection GetBoltConnection(this UdpConnection self) {
    return (BoltConnection)self.UserToken;
  }

  public static string Join<T>(this IEnumerable<T> items, string seperator) {
    return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
  }

  public static bool ViewPointIsOnScreen(this Vector3 vp) {
    return vp.z >= 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
  }

  public static T[] CloneArray<T>(this T[] array) {
    T[] clone = new T[array.Length];
    Array.Copy(array, 0, clone, 0, array.Length);
    return clone;
  }

  public static T[] AddFirst<T>(this T[] array, T item) {
    if (array == null) {
      return new T[1] { item };
    }

    // duplicate + 1 extra slot
    T[] clone = new T[array.Length + 1];

    // copy old items to index 1 ... n
    Array.Copy(array, 0, clone, 1, array.Length);

    // insert new item at index 0
    clone[0] = item;

    return clone;
  }

  public static void WriteUniqueId(this UdpPacket stream, UniqueId id) {
    stream.WriteUInt(id.uint0);
    stream.WriteUInt(id.uint1);
    stream.WriteUInt(id.uint2);
    stream.WriteUInt(id.uint3);
  }

  public static UniqueId ReadUniqueId(this UdpPacket stream) {
    UniqueId id;

    id = default(UniqueId);
    id.uint0 = stream.ReadUInt();
    id.uint1 = stream.ReadUInt();
    id.uint2 = stream.ReadUInt();
    id.uint3 = stream.ReadUInt();

    return id;
  }


  public static void WriteByteArraySimple(this UdpPacket stream, byte[] array, int maxLength) {
    if (stream.WriteBool(array != null)) {
      int length = Mathf.Min(array.Length, maxLength);

      if (length < array.Length) {
        BoltLog.Warn("Only sendig {0}/{1} bytes from byte array", length, array.Length);
      }

      stream.WriteUShort((ushort)length);
      stream.WriteByteArray(array, 0, length);
    }
  }

  public static byte[] ReadByteArraySimple(this UdpPacket stream) {
    if (stream.ReadBool()) {
      int length = stream.ReadUShort();
      byte[] data = new byte[length];

      stream.ReadByteArray(data, 0, data.Length);

      return data;
    }
    else {
      return null;
    }
  }

  public static void WriteColor32RGBA(this UdpPacket stream, Color32 value) {
    stream.WriteByte(value.r, 8);
    stream.WriteByte(value.g, 8);
    stream.WriteByte(value.b, 8);
    stream.WriteByte(value.a, 8);
  }

  public static Color32 ReadColor32RGBA(this UdpPacket stream) {
    return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8));
  }

  public static void WriteColor32RGB(this UdpPacket stream, Color32 value) {
    stream.WriteByte(value.r, 8);
    stream.WriteByte(value.g, 8);
    stream.WriteByte(value.b, 8);
  }

  public static Color32 ReadColor32RGB(this UdpPacket stream) {
    return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), 0xFF);
  }

  public static void WriteVector2(this UdpPacket stream, Vector2 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
  }

  public static Vector2 ReadVector2(this UdpPacket stream) {
    return new Vector2(stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteVector3(this UdpPacket stream, Vector3 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
  }

  public static Vector3 ReadVector3(this UdpPacket stream) {
    return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteColorRGB(this UdpPacket stream, Color value) {
    stream.WriteFloat(value.r);
    stream.WriteFloat(value.g);
    stream.WriteFloat(value.b);
  }

  public static Color ReadColorRGB(this UdpPacket stream) {
    return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteVector4(this UdpPacket stream, Vector4 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
    stream.WriteFloat(value.w);
  }

  public static Vector4 ReadVector4(this UdpPacket stream) {
    return new Vector4(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }
  public static void WriteColorRGBA(this UdpPacket stream, Color value) {
    stream.WriteFloat(value.r);
    stream.WriteFloat(value.g);
    stream.WriteFloat(value.b);
    stream.WriteFloat(value.a);
  }

  public static Color ReadColorRGBA(this UdpPacket stream) {
    return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }

  public static void WriteQuaternion(this UdpPacket stream, Quaternion value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
    stream.WriteFloat(value.w);
  }

  public static Quaternion ReadQuaternion(this UdpPacket stream) {
    return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }

  public static void WriteTransform(this UdpPacket stream, Transform transform) {
    BoltUtils.WriteVector3(stream, transform.position);
    BoltUtils.WriteQuaternion(stream, transform.rotation);
  }

  public static void ReadTransform(this UdpPacket stream, Transform transform) {
    transform.position = BoltUtils.ReadVector3(stream);
    transform.rotation = BoltUtils.ReadQuaternion(stream);
  }

  public static void ReadTransform(this UdpPacket stream, out Vector3 position, out Quaternion rotation) {
    position = BoltUtils.ReadVector3(stream);
    rotation = BoltUtils.ReadQuaternion(stream);
  }


  public static void WriteRigidbody(this UdpPacket stream, Rigidbody rigidbody) {
    BoltUtils.WriteVector3(stream, rigidbody.position);
    BoltUtils.WriteQuaternion(stream, rigidbody.rotation);
    BoltUtils.WriteVector3(stream, rigidbody.velocity);
    BoltUtils.WriteVector3(stream, rigidbody.angularVelocity);
  }

  public static void ReadRigidbody(this UdpPacket stream, Rigidbody rigidbody) {
    rigidbody.position = BoltUtils.ReadVector3(stream);
    rigidbody.rotation = BoltUtils.ReadQuaternion(stream);
    rigidbody.velocity = BoltUtils.ReadVector3(stream);
    rigidbody.angularVelocity = BoltUtils.ReadVector3(stream);
  }

  public static void ReadRigidbody(this UdpPacket stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity) {
    position = BoltUtils.ReadVector3(stream);
    rotation = BoltUtils.ReadQuaternion(stream);
    velocity = BoltUtils.ReadVector3(stream);
    angularVelocity = BoltUtils.ReadVector3(stream);
  }

  public static void WriteBounds(this UdpPacket stream, Bounds b) {
    BoltUtils.WriteVector3(stream, b.center);
    BoltUtils.WriteVector3(stream, b.size);
  }

  public static Bounds ReadBounds(this UdpPacket stream) {
    return new Bounds(BoltUtils.ReadVector3(stream), BoltUtils.ReadVector3(stream));
  }

  public static void WriteRect(this UdpPacket stream, Rect rect) {
    stream.WriteFloat(rect.xMin);
    stream.WriteFloat(rect.yMin);
    stream.WriteFloat(rect.width);
    stream.WriteFloat(rect.height);
  }

  public static Rect ReadRect(this UdpPacket stream) {
    return new Rect(
        stream.ReadFloat(),
        stream.ReadFloat(),
        stream.ReadFloat(),
        stream.ReadFloat()
    );
  }


  public static void WriteRay(this UdpPacket stream, Ray ray) {
    BoltUtils.WriteVector3(stream, ray.origin);
    BoltUtils.WriteVector3(stream, ray.direction);
  }

  public static Ray ReadRay(this UdpPacket stream) {
    return new Ray(
        BoltUtils.ReadVector3(stream),
        BoltUtils.ReadVector3(stream)
    );
  }


  public static void WritePlane(this UdpPacket stream, Plane plane) {
    BoltUtils.WriteVector3(stream, plane.normal);
    stream.WriteFloat(plane.distance);
  }

  public static Plane ReadPlane(this UdpPacket stream) {
    return new Plane(
      BoltUtils.ReadVector3(stream),
      stream.ReadFloat()
    );
  }


  public static void WriteLayerMask(this UdpPacket stream, LayerMask mask) {
    stream.WriteInt(mask.value, 32);
  }

  public static LayerMask ReadLayerMask(this UdpPacket stream) {
    return stream.ReadInt(32);
  }

  public static void WriteMatrix4x4(this UdpPacket stream, ref Matrix4x4 m) {
    stream.WriteFloat(m.m00);
    stream.WriteFloat(m.m01);
    stream.WriteFloat(m.m02);
    stream.WriteFloat(m.m03);
    stream.WriteFloat(m.m10);
    stream.WriteFloat(m.m11);
    stream.WriteFloat(m.m12);
    stream.WriteFloat(m.m13);
    stream.WriteFloat(m.m20);
    stream.WriteFloat(m.m21);
    stream.WriteFloat(m.m22);
    stream.WriteFloat(m.m23);
    stream.WriteFloat(m.m30);
    stream.WriteFloat(m.m31);
    stream.WriteFloat(m.m32);
    stream.WriteFloat(m.m33);
  }

  public static Matrix4x4 ReadMatrix4x4(this UdpPacket stream) {
    Matrix4x4 m = default(Matrix4x4);
    m.m00 = stream.ReadFloat();
    m.m01 = stream.ReadFloat();
    m.m02 = stream.ReadFloat();
    m.m03 = stream.ReadFloat();
    m.m10 = stream.ReadFloat();
    m.m11 = stream.ReadFloat();
    m.m12 = stream.ReadFloat();
    m.m13 = stream.ReadFloat();
    m.m20 = stream.ReadFloat();
    m.m21 = stream.ReadFloat();
    m.m22 = stream.ReadFloat();
    m.m23 = stream.ReadFloat();
    m.m30 = stream.ReadFloat();
    m.m31 = stream.ReadFloat();
    m.m32 = stream.ReadFloat();
    m.m33 = stream.ReadFloat();
    return m;
  }

  public static void WriteIntVB(this UdpPacket packet, int v) {
    packet.WriteUIntVB((uint)v);
  }

  public static int ReadIntVB(this UdpPacket packet) {
    return (int)packet.ReadUIntVB();
  }

  public static void WriteUIntVB(this UdpPacket packet, uint v) {
    uint b = 0U;

    do {
      b = v & 127U;
      v = v >> 7;

      if (v > 0) {
        b |= 128U;
      }

      packet.WriteByte((byte)b);
    } while (v != 0);
  }

  public static uint ReadUIntVB(this UdpPacket packet) {
    uint v = 0U;
    uint b = 0U;

    int s = 0;

    do {
      b = packet.ReadByte();
      v = v | ((b & 127U) << s);
      s = s + 7;

    } while (b > 127U);

    return v;
  }

  internal static void WriteEntity(this UdpPacket packet, Entity entity) {
    if (packet.WriteBool((entity != null) && entity.IsAttached)) {
      packet.WriteNetworkId(entity.NetworkId);
    }
  }

  internal static Entity ReadEntity(this UdpPacket packet) {
    if (packet.ReadBool()) {
      return BoltCore.FindEntity(packet.ReadNetworkId());
    }

    return null;
  }

  public static void WriteNetworkId(this UdpPacket packet, NetworkId id) {
    Assert.True(id.Connection != uint.MaxValue);
    packet.WriteUIntVB(id.Connection);
    packet.WriteUIntVB(id.Entity);
  }

  public static NetworkId ReadNetworkId(this UdpPacket packet) {
    uint connection = packet.ReadUIntVB();
    uint entity = packet.ReadUIntVB();
    Assert.True(connection != uint.MaxValue);
    return new NetworkId(connection, entity);
  }

  internal static void WriteContinueMarker(this UdpPacket stream) {
    if (stream.CanWrite()) {
      stream.WriteBool(true);
    }
  }

  internal static void WriteStopMarker(this UdpPacket stream) {
    if (stream.CanWrite()) {
      stream.WriteBool(false);
    }
  }

  internal static bool ReadStopMarker(this UdpPacket stream) {
    if (stream.CanRead()) {
      return stream.ReadBool();
    }

    return false;
  }
  static void ByteToString(byte value, StringBuilder sb) {
    ByteToString(value, 8, sb);
  }

  static void ByteToString(byte value, int bits, StringBuilder sb) {
#if DEBUG
    if (bits < 1 || bits > 8) {
      throw new ArgumentOutOfRangeException("bits", "Must be between 1 and 8");
    }
#endif

    for (int i = (bits - 1); i >= 0; --i) {
      if (((1 << i) & value) == 0) {
        sb.Append('0');
      }
      else {
        sb.Append('1');
      }
    }
  }

  public static string ByteToString(byte value, int bits) {
    StringBuilder sb = new StringBuilder(8);
    ByteToString(value, bits, sb);
    return sb.ToString();
  }

  public static string ByteToString(byte value) {
    return ByteToString(value, 8);
  }

  public static string UShortToString(ushort value) {
    StringBuilder sb = new StringBuilder(17);

    ByteToString((byte)(value >> 8), sb);
    sb.Append(' ');
    ByteToString((byte)value, sb);

    return sb.ToString();
  }

  public static string IntToString(int value) {
    return UIntToString((uint)value);
  }

  public static string UIntToString(uint value) {
    StringBuilder sb = new StringBuilder(35);

    ByteToString((byte)(value >> 24), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 16), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 8), sb);
    sb.Append(' ');
    ByteToString((byte)value, sb);

    return sb.ToString();
  }

  public static string ULongToString(ulong value) {
    StringBuilder sb = new StringBuilder(71);

    ByteToString((byte)(value >> 56), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 48), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 40), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 32), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 24), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 16), sb);
    sb.Append(' ');
    ByteToString((byte)(value >> 8), sb);
    sb.Append(' ');
    ByteToString((byte)value, sb);

    return sb.ToString();
  }

  public static string BytesToString(byte[] values) {
    StringBuilder sb = new StringBuilder(
        (values.Length * 8) + System.Math.Max(0, (values.Length - 1))
    );

    for (int i = values.Length - 1; i >= 0; --i) {
      sb.Append(ByteToString(values[i]));

      if (i != 0) {
        sb.Append(' ');
      }
    }

    return sb.ToString();
  }
}
