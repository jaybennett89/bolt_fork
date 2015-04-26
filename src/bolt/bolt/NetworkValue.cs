using System;
using System.Runtime.InteropServices;

namespace Bolt {
  [StructLayout(LayoutKind.Explicit)]
  internal struct NetworkValue {
    [FieldOffset(0)]
    public System.UInt64 Packed0;

    [FieldOffset(8)]
    public System.UInt64 Packed1;

    [FieldOffset(0)]
    public System.Boolean Bool;

    [FieldOffset(0)]
    public System.Int32 Int0;

    [FieldOffset(0)]
    public System.Single Float0;

    [FieldOffset(4)]
    public System.Single Float1;

    [FieldOffset(8)]
    public System.Single Float2;

    [FieldOffset(12)]
    public System.Single Float3;

    [FieldOffset(0)]
    public System.Guid Guid;

    [FieldOffset(0)]
    public Bolt.NetworkId NetworkId;

    [FieldOffset(0)]
    public Bolt.PrefabId PrefabId;

    [FieldOffset(0)]
    public Bolt.NetworkTrigger TriggerLocal;

    [FieldOffset(8)]
    public Bolt.NetworkTrigger TriggerSend;

    [FieldOffset(0)]
    public UnityEngine.Vector2 Vector2;

    [FieldOffset(0)]
    public UnityEngine.Vector3 Vector3;

    [FieldOffset(0)]
    public UnityEngine.Quaternion Quaternion;

    [FieldOffset(0)]
    public UnityEngine.Color Color;

    [FieldOffset(16)]
    public System.Object Object;

    public bool HasNonDefaultValue {
      get { return (Packed0 != 0) || (Packed1 != 0) || (Object != null); }
    }

    public UnityEngine.Matrix4x4 Matrix4x4 {
      get { return (UnityEngine.Matrix4x4)Object; }
      set { Object = value; }
    }

    public System.String String {
      get { return (System.String)Object; }
      set { Object = value; }
    }

    public Bolt.IProtocolToken ProtocolToken {
      get { return (Bolt.IProtocolToken)Object; }
      set { Object = value; }
    }

    public Bolt.NetworkTransform Transform {
      get { return (Bolt.NetworkTransform)Object; }
      set { Object = value; }
    }

    public System.Action Action {
      get { return (System.Action)Object; }
      set { Object = value; }
    }

    public BoltEntity Entity {
      get {
        return BoltNetwork.FindEntity(NetworkId);
      }
      set {
        if (value && value.isAttached) {
          NetworkId = value.networkId;
        }
        else {
          NetworkId = new NetworkId();
        }
      }
    }

    public static bool Diff(Boolean a, Boolean b) { return a != b; }
    public static bool Diff(Int32 a, Int32 b) { return a != b; }
    public static bool Diff(Guid a, Guid b) { return a != b; }
    public static bool Diff(Single a, Single b) { return a != b; }
    public static bool Diff(NetworkId a, NetworkId b) { return a != b; }
    public static bool Diff(PrefabId a, PrefabId b) { return a != b; }
    public static bool Diff(NetworkTrigger a, NetworkTrigger b) { return a != b; }
    public static bool Diff(System.Object a, System.Object b) { return ReferenceEquals(a, b) == false; }

    public static bool Diff(UnityEngine.Vector2 a, UnityEngine.Vector2 b) { return a != b; }
    public static bool Diff(UnityEngine.Vector3 a, UnityEngine.Vector3 b) { return a != b; }
    public static bool Diff(UnityEngine.Quaternion a, UnityEngine.Quaternion b) { return a != b; }
    public static bool Diff(UnityEngine.Color a, UnityEngine.Color b) { return a != b; }

    public static bool Diff_Strict(UnityEngine.Vector2 a, UnityEngine.Vector2 b) {
      return a.x != b.x || a.y != b.y;
    }

    public static bool Diff_Strict(UnityEngine.Vector3 a, UnityEngine.Vector3 b) {
      return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static bool Diff_Strict(UnityEngine.Quaternion a, UnityEngine.Quaternion b) {
      return a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;
    }
  }
}
