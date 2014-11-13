using Bolt;
using UdpKit;
using UnityEngine;

public static class UdpStreamExtensions {
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
    UdpStreamExtensions.WriteVector3(stream, transform.position);
    UdpStreamExtensions.WriteQuaternion(stream, transform.rotation);
  }

  public static void ReadTransform(this UdpPacket stream, Transform transform) {
    transform.position = UdpStreamExtensions.ReadVector3(stream);
    transform.rotation = UdpStreamExtensions.ReadQuaternion(stream);
  }

  public static void ReadTransform(this UdpPacket stream, out Vector3 position, out Quaternion rotation) {
    position = UdpStreamExtensions.ReadVector3(stream);
    rotation = UdpStreamExtensions.ReadQuaternion(stream);
  }


  public static void WriteRigidbody(this UdpPacket stream, Rigidbody rigidbody) {
    UdpStreamExtensions.WriteVector3(stream, rigidbody.position);
    UdpStreamExtensions.WriteQuaternion(stream, rigidbody.rotation);
    UdpStreamExtensions.WriteVector3(stream, rigidbody.velocity);
    UdpStreamExtensions.WriteVector3(stream, rigidbody.angularVelocity);
  }

  public static void ReadRigidbody(this UdpPacket stream, Rigidbody rigidbody) {
    rigidbody.position = UdpStreamExtensions.ReadVector3(stream);
    rigidbody.rotation = UdpStreamExtensions.ReadQuaternion(stream);
    rigidbody.velocity = UdpStreamExtensions.ReadVector3(stream);
    rigidbody.angularVelocity = UdpStreamExtensions.ReadVector3(stream);
  }

  public static void ReadRigidbody(this UdpPacket stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity) {
    position = UdpStreamExtensions.ReadVector3(stream);
    rotation = UdpStreamExtensions.ReadQuaternion(stream);
    velocity = UdpStreamExtensions.ReadVector3(stream);
    angularVelocity = UdpStreamExtensions.ReadVector3(stream);
  }

  public static void WriteBounds(this UdpPacket stream, Bounds b) {
    UdpStreamExtensions.WriteVector3(stream, b.center);
    UdpStreamExtensions.WriteVector3(stream, b.size);
  }

  public static Bounds ReadBounds(this UdpPacket stream) {
    return new Bounds(UdpStreamExtensions.ReadVector3(stream), UdpStreamExtensions.ReadVector3(stream));
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
    UdpStreamExtensions.WriteVector3(stream, ray.origin);
    UdpStreamExtensions.WriteVector3(stream, ray.direction);
  }

  public static Ray ReadRay(this UdpPacket stream) {
    return new Ray(
        UdpStreamExtensions.ReadVector3(stream),
        UdpStreamExtensions.ReadVector3(stream)
    );
  }


  public static void WritePlane(this UdpPacket stream, Plane plane) {
    UdpStreamExtensions.WriteVector3(stream, plane.normal);
    stream.WriteFloat(plane.distance);
  }

  public static Plane ReadPlane(this UdpPacket stream) {
    return new Plane(
      UdpStreamExtensions.ReadVector3(stream),
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

  //internal static void WriteNetworkId_old(this UdpPacket stream, NetId id) {
  //  Assert.True(id.Value >= 0);
  //  Assert.True(id.Value < EntityProxy.MAX_COUNT);

  //  stream.WriteInt(id.Value, EntityProxy.ID_BIT_COUNT);
  //}

  //internal static NetId ReadNetworkId_Old(this UdpPacket stream) {
  //  return new NetId(stream.ReadInt(EntityProxy.ID_BIT_COUNT));
  //}

  //internal static void WriteEntity(this UdpPacket stream, Entity en, BoltConnection cn) {
  //  if (en == null) {
  //    stream.WriteBool(false);
  //    return;
  //  }

  //  NetId id = cn.GetNetworkId(en);

  //  // one bit if we have it or not, at all
  //  if (stream.WriteBool(id.Value != int.MaxValue)) {
  //    // one bit which significes if this is an outgoing or incomming proxy
  //    // if it's an incomming that means the sourceConnection is the same as this connection
  //    // if it's an outgoing that means the sourceConnection is either null (it's local) 
  //    // or it's a connection which which we received this object from
  //    stream.WriteBool(ReferenceEquals(en.Source, cn));

  //    // the actual network id
  //    stream.WriteNetworkId_old(id);
  //  }
  //}

  //internal static Entity ReadEntity(this UdpPacket stream, BoltConnection cn) {
  //  NetId networkId;
  //  return ReadEntity(stream, cn, out networkId);
  //}

  //internal static Entity ReadEntity(this UdpPacket stream, BoltConnection cn, out NetId networkId) {
  //  if (stream.ReadBool()) {
  //    // if this bool reads true, that means that the
  //    // other end of the connection classifices this 
  //    // entity as an incomming, which means it's outgoing for us
  //    // and the reverse if it's false

  //    if (stream.ReadBool()) {
  //      networkId = stream.ReadNetworkId_Old();
  //      return cn.GetOutgoingEntity(networkId);
  //    }
  //    else {
  //      networkId = stream.ReadNetworkId_Old();
  //      return cn.GetIncommingEntity(networkId);
  //    }
  //  }
  //  else {
  //    networkId = new NetId(int.MaxValue);
  //    return null;
  //  }
  //}

  //public static void PackUIntVB(this UdpPacket packet, uint v) {
  //  uint b = 0U;

  //  do {
  //    b = v & 127U;
  //    v = v >> 7;

  //    if (v > 0) {
  //      b |= 128U;
  //    }

  //    packet.WriteByte((byte)b);
  //  } while ((b & 128U) == 128U);
  //}

  //public static uint ReadUIntVB(this UdpPacket packet) {
  //  uint v = 0U;
  //  uint b = 0U;

  //  int s = 0;

  //  do {
  //    b = packet.ReadByte();
  //    v = v | ((b & 127U) << s);
  //    s = s + 7;

  //  } while ((b & 128U) == 128U);

  //  return v;
  //}

  internal static void WriteEntity(this UdpPacket packet, Entity entity) {
    if (packet.WriteBool((entity != null) && entity.IsAttached)) {
      packet.PackNetworkId(entity.NetworkId);
    }
  }

  internal static Entity ReadEntity(this UdpPacket packet) {
    if (packet.ReadBool()) {
      return BoltCore.FindEntity(packet.ReadNetworkId());
    }

    return null;
  }

  public static void PackNetworkId(this UdpPacket packet, NetworkId id) {
    packet.WriteUInt(id.Connection);
    packet.WriteUInt(id.Entity);
  }

  public static NetworkId ReadNetworkId(this UdpPacket packet) {
    uint connection = packet.ReadUInt();
    uint entity = packet.ReadUInt();
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
}
