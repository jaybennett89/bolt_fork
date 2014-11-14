using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Bolt {
  [StructLayout(LayoutKind.Explicit)]
  internal struct NetworkValue {
    [FieldOffset(0)]
    public System.Boolean Boolean;

    [FieldOffset(0)]
    public System.Int32 Int32;

    [FieldOffset(0)]
    public System.Single Single;

    [FieldOffset(0)]
    public Bolt.NetworkId NetworkId;

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

    public System.String String {
      get { return (System.String)Object; }
      set { Object = value; }
    }

    //public Bolt.INetworkTransform INetworkTransform {
    //  get { return (Bolt.INetworkTransform)Object; }
    //  set { Object = value; }
    //}

    //public INetworkObject INetworkObject {
    //  get {
    //    var obj = (INetworkObject)Object;
    //    if (NetworkId.Matches(obj)) {
    //      return obj;
    //    }

    //    return null;
    //  }
    //  set {
    //    Object = value;
    //    NetworkId = ((INetworkObjectInternal)value).MetaObject.NetworkId;
    //  }
    //}

    //public INetworkCollection INetworkCollection {
    //  get {
    //    var obj = (INetworkCollection)Object;
    //    if (NetworkId.Matches(obj)) {
    //      return obj;
    //    }

    //    return null;
    //  }
    //  set {
    //    Object = value;
    //    NetworkId = value.Id;
    //  }
    //}
  }
}
