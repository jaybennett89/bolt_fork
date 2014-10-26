using Bolt;
using UdpKit;
using UnityEngine;

public static class UdpStreamExtensions {
  public static void WriteByteArraySimple(this UdpStream stream, byte[] array, int maxLength) {
    if (stream.WriteBool(array != null)) {
      int length = Mathf.Min(array.Length, maxLength);

      if (length < array.Length) {
        BoltLog.Warn("Only sendig {0}/{1} bytes from byte array", length, array.Length);
      }

      stream.WriteUShort((ushort)length);
      stream.WriteByteArray(array, 0, length);
    }
  }

  public static byte[] ReadByteArraySimple(this UdpStream stream) {
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

  public static void WriteColor32RGBA(this UdpStream stream, Color32 value) {
    stream.WriteByte(value.r, 8);
    stream.WriteByte(value.g, 8);
    stream.WriteByte(value.b, 8);
    stream.WriteByte(value.a, 8);
  }

  public static Color32 ReadColor32RGBA(this UdpStream stream) {
    return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8));
  }

  public static void WriteColor32RGB(this UdpStream stream, Color32 value) {
    stream.WriteByte(value.r, 8);
    stream.WriteByte(value.g, 8);
    stream.WriteByte(value.b, 8);
  }

  public static Color32 ReadColor32RGB(this UdpStream stream) {
    return new Color32(stream.ReadByte(8), stream.ReadByte(8), stream.ReadByte(8), 0xFF);
  }

  public static void WriteVector2(this UdpStream stream, Vector2 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
  }

  public static Vector2 ReadVector2(this UdpStream stream) {
    return new Vector2(stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteVector3(this UdpStream stream, Vector3 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
  }

  public static Vector3 ReadVector3(this UdpStream stream) {
    return new Vector3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteColorRGB(this UdpStream stream, Color value) {
    stream.WriteFloat(value.r);
    stream.WriteFloat(value.g);
    stream.WriteFloat(value.b);
  }

  public static Color ReadColorRGB(this UdpStream stream) {
    return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }


  public static void WriteVector4(this UdpStream stream, Vector4 value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
    stream.WriteFloat(value.w);
  }

  public static Vector4 ReadVector4(this UdpStream stream) {
    return new Vector4(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }
  public static void WriteColorRGBA(this UdpStream stream, Color value) {
    stream.WriteFloat(value.r);
    stream.WriteFloat(value.g);
    stream.WriteFloat(value.b);
    stream.WriteFloat(value.a);
  }

  public static Color ReadColorRGBA(this UdpStream stream) {
    return new Color(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }

  public static void WriteQuaternion(this UdpStream stream, Quaternion value) {
    stream.WriteFloat(value.x);
    stream.WriteFloat(value.y);
    stream.WriteFloat(value.z);
    stream.WriteFloat(value.w);
  }

  public static Quaternion ReadQuaternion(this UdpStream stream) {
    return new Quaternion(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
  }

  public static void WriteTransform(this UdpStream stream, Transform transform) {
    UdpStreamExtensions.WriteVector3(stream, transform.position);
    UdpStreamExtensions.WriteQuaternion(stream, transform.rotation);
  }

  public static void ReadTransform(this UdpStream stream, Transform transform) {
    transform.position = UdpStreamExtensions.ReadVector3(stream);
    transform.rotation = UdpStreamExtensions.ReadQuaternion(stream);
  }

  public static void ReadTransform(this UdpStream stream, out Vector3 position, out Quaternion rotation) {
    position = UdpStreamExtensions.ReadVector3(stream);
    rotation = UdpStreamExtensions.ReadQuaternion(stream);
  }


  public static void WriteRigidbody(this UdpStream stream, Rigidbody rigidbody) {
    UdpStreamExtensions.WriteVector3(stream, rigidbody.position);
    UdpStreamExtensions.WriteQuaternion(stream, rigidbody.rotation);
    UdpStreamExtensions.WriteVector3(stream, rigidbody.velocity);
    UdpStreamExtensions.WriteVector3(stream, rigidbody.angularVelocity);
  }

  public static void ReadRigidbody(this UdpStream stream, Rigidbody rigidbody) {
    rigidbody.position = UdpStreamExtensions.ReadVector3(stream);
    rigidbody.rotation = UdpStreamExtensions.ReadQuaternion(stream);
    rigidbody.velocity = UdpStreamExtensions.ReadVector3(stream);
    rigidbody.angularVelocity = UdpStreamExtensions.ReadVector3(stream);
  }

  public static void ReadRigidbody(this UdpStream stream, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity) {
    position = UdpStreamExtensions.ReadVector3(stream);
    rotation = UdpStreamExtensions.ReadQuaternion(stream);
    velocity = UdpStreamExtensions.ReadVector3(stream);
    angularVelocity = UdpStreamExtensions.ReadVector3(stream);
  }

  public static void WriteBounds(this UdpStream stream, Bounds b) {
    UdpStreamExtensions.WriteVector3(stream, b.center);
    UdpStreamExtensions.WriteVector3(stream, b.size);
  }

  public static Bounds ReadBounds(this UdpStream stream) {
    return new Bounds(UdpStreamExtensions.ReadVector3(stream), UdpStreamExtensions.ReadVector3(stream));
  }

  public static void WriteRect(this UdpStream stream, Rect rect) {
    stream.WriteFloat(rect.xMin);
    stream.WriteFloat(rect.yMin);
    stream.WriteFloat(rect.width);
    stream.WriteFloat(rect.height);
  }

  public static Rect ReadRect(this UdpStream stream) {
    return new Rect(
        stream.ReadFloat(),
        stream.ReadFloat(),
        stream.ReadFloat(),
        stream.ReadFloat()
    );
  }


  public static void WriteRay(this UdpStream stream, Ray ray) {
    UdpStreamExtensions.WriteVector3(stream, ray.origin);
    UdpStreamExtensions.WriteVector3(stream, ray.direction);
  }

  public static Ray ReadRay(this UdpStream stream) {
    return new Ray(
        UdpStreamExtensions.ReadVector3(stream),
        UdpStreamExtensions.ReadVector3(stream)
    );
  }


  public static void WritePlane(this UdpStream stream, Plane plane) {
    UdpStreamExtensions.WriteVector3(stream, plane.normal);
    stream.WriteFloat(plane.distance);
  }

  public static Plane ReadPlane(this UdpStream stream) {
    return new Plane(
      UdpStreamExtensions.ReadVector3(stream),
      stream.ReadFloat()
    );
  }


  public static void WriteLayerMask(this UdpStream stream, LayerMask mask) {
    stream.WriteInt(mask.value, 32);
  }

  public static LayerMask ReadLayerMask(this UdpStream stream) {
    return stream.ReadInt(32);
  }

  public static void WriteMatrix4x4(this UdpStream stream, ref Matrix4x4 m) {
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

  public static Matrix4x4 ReadMatrix4x4(this UdpStream stream) {
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

  internal static void WriteNetworkId(this UdpStream stream, NetId id) {
    Assert.True(id.Value >= 0);
    Assert.True(id.Value < EntityProxy.MAX_COUNT);

    stream.WriteInt(id.Value, EntityProxy.ID_BIT_COUNT);
  }

  internal static NetId ReadNetworkId(this UdpStream stream) {
    return new NetId(stream.ReadInt(EntityProxy.ID_BIT_COUNT));
  }

  internal static void WriteEntity(this UdpStream stream, Entity en, BoltConnection cn) {
    if (en == null) {
      stream.WriteBool(false);
      return;
    }

    NetId id = cn.GetNetworkId(en);

    // one bit if we have it or not, at all
    if (stream.WriteBool(id.Value != int.MaxValue)) {
      // one bit which significes if this is an outgoing or incomming proxy
      // if it's an incomming that means the sourceConnection is the same as this connection
      // if it's an outgoing that means the sourceConnection is either null (it's local) 
      // or it's a connection which which we received this object from
      stream.WriteBool(ReferenceEquals(en.Source, cn));

      // the actual network id
      stream.WriteNetworkId(id);
    }
  }

  internal static Entity ReadEntity(this UdpStream stream, BoltConnection cn) {
    NetId networkId;
    return ReadEntity(stream, cn, out networkId);
  }

  internal static Entity ReadEntity(this UdpStream stream, BoltConnection cn, out NetId networkId) {
    if (stream.ReadBool()) {
      // if this bool reads true, that means that the
      // other end of the connection classifices this 
      // entity as an incomming, which means it's outgoing for us
      // and the reverse if it's false

      if (stream.ReadBool()) {
        networkId = stream.ReadNetworkId();
        return cn.GetOutgoingEntity(networkId);
      }
      else {
        networkId = stream.ReadNetworkId();
        return cn.GetIncommingEntity(networkId);
      }
    }
    else {
      networkId = new NetId(int.MaxValue);
      return null;
    }
  }

  internal static void WriteContinueMarker(this UdpStream stream) {
    if (stream.CanWrite()) {
      stream.WriteBool(true);
    }
  }

  internal static void WriteStopMarker(this UdpStream stream) {
    if (stream.CanWrite()) {
      stream.WriteBool(false);
    }
  }

  internal static bool ReadStopMarker(this UdpStream stream) {
    if (stream.CanRead()) {
      return stream.ReadBool();
    }

    return false;
  }
}
