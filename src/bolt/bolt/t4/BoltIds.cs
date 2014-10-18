using System.Collections.Generic;

namespace Bolt {
[Documentation]
  public struct NetId {
    public class Comparer : IComparer<NetId> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<NetId>.Compare(NetId x, NetId y) {
        return x.Value - y.Value;
      }
    }

    public class EqualityComparer : IEqualityComparer<NetId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<NetId>.Equals(NetId x, NetId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<NetId>.GetHashCode(NetId x) {
        return x.Value;
      }
    }

    public readonly int Value;

    internal NetId(int value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj is NetId) {
        return this.Value == ((NetId)obj).Value;
      }

      return false;
    }

    public override int GetHashCode() {
      return Value;
    }

    public override string ToString() {
      return string.Format("[NetId:{0}]", Value);
    }

	public void Pack(UdpKit.UdpStream stream, int bits) {
		stream.WriteInt(Value, bits);
	}
	
	public void Pack(UdpKit.UdpStream stream) {
		stream.WriteInt(Value);
	}

	public static NetId Read(UdpKit.UdpStream stream, int bits) {
		return new NetId(stream.ReadInt(bits));
	}
	
	public static NetId Read(UdpKit.UdpStream stream) {
		return new NetId(stream.ReadInt());
	}

    public static bool operator ==(NetId a, NetId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(NetId a, NetId b) {
      return a.Value != b.Value;
    }
  }
}

namespace Bolt {
[Documentation]
  public struct InstanceId {
    public class Comparer : IComparer<InstanceId> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<InstanceId>.Compare(InstanceId x, InstanceId y) {
        return x.Value - y.Value;
      }
    }

    public class EqualityComparer : IEqualityComparer<InstanceId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<InstanceId>.Equals(InstanceId x, InstanceId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<InstanceId>.GetHashCode(InstanceId x) {
        return x.Value;
      }
    }

    public readonly int Value;

    internal InstanceId(int value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj is InstanceId) {
        return this.Value == ((InstanceId)obj).Value;
      }

      return false;
    }

    public override int GetHashCode() {
      return Value;
    }

    public override string ToString() {
      return string.Format("[InstanceId:{0}]", Value);
    }

	public void Pack(UdpKit.UdpStream stream, int bits) {
		stream.WriteInt(Value, bits);
	}
	
	public void Pack(UdpKit.UdpStream stream) {
		stream.WriteInt(Value);
	}

	public static InstanceId Read(UdpKit.UdpStream stream, int bits) {
		return new InstanceId(stream.ReadInt(bits));
	}
	
	public static InstanceId Read(UdpKit.UdpStream stream) {
		return new InstanceId(stream.ReadInt());
	}

    public static bool operator ==(InstanceId a, InstanceId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(InstanceId a, InstanceId b) {
      return a.Value != b.Value;
    }
  }
}

namespace Bolt {
[Documentation]
  public struct PrefabId {
    public class Comparer : IComparer<PrefabId> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<PrefabId>.Compare(PrefabId x, PrefabId y) {
        return x.Value - y.Value;
      }
    }

    public class EqualityComparer : IEqualityComparer<PrefabId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<PrefabId>.Equals(PrefabId x, PrefabId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<PrefabId>.GetHashCode(PrefabId x) {
        return x.Value;
      }
    }

    public readonly int Value;

    internal PrefabId(int value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj is PrefabId) {
        return this.Value == ((PrefabId)obj).Value;
      }

      return false;
    }

    public override int GetHashCode() {
      return Value;
    }

    public override string ToString() {
      return string.Format("[PrefabId:{0}]", Value);
    }

	public void Pack(UdpKit.UdpStream stream, int bits) {
		stream.WriteInt(Value, bits);
	}
	
	public void Pack(UdpKit.UdpStream stream) {
		stream.WriteInt(Value);
	}

	public static PrefabId Read(UdpKit.UdpStream stream, int bits) {
		return new PrefabId(stream.ReadInt(bits));
	}
	
	public static PrefabId Read(UdpKit.UdpStream stream) {
		return new PrefabId(stream.ReadInt());
	}

    public static bool operator ==(PrefabId a, PrefabId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(PrefabId a, PrefabId b) {
      return a.Value != b.Value;
    }
  }
}

namespace Bolt {
[Documentation]
  public struct TypeId {
    public class Comparer : IComparer<TypeId> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<TypeId>.Compare(TypeId x, TypeId y) {
        return x.Value - y.Value;
      }
    }

    public class EqualityComparer : IEqualityComparer<TypeId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<TypeId>.Equals(TypeId x, TypeId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<TypeId>.GetHashCode(TypeId x) {
        return x.Value;
      }
    }

    public readonly int Value;

    internal TypeId(int value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj is TypeId) {
        return this.Value == ((TypeId)obj).Value;
      }

      return false;
    }

    public override int GetHashCode() {
      return Value;
    }

    public override string ToString() {
      return string.Format("[TypeId:{0}]", Value);
    }

	public void Pack(UdpKit.UdpStream stream, int bits) {
		stream.WriteInt(Value, bits);
	}
	
	public void Pack(UdpKit.UdpStream stream) {
		stream.WriteInt(Value);
	}

	public static TypeId Read(UdpKit.UdpStream stream, int bits) {
		return new TypeId(stream.ReadInt(bits));
	}
	
	public static TypeId Read(UdpKit.UdpStream stream) {
		return new TypeId(stream.ReadInt());
	}

    public static bool operator ==(TypeId a, TypeId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(TypeId a, TypeId b) {
      return a.Value != b.Value;
    }
  }
}

namespace Bolt {
[Documentation]
  public struct SceneId {
    public class Comparer : IComparer<SceneId> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<SceneId>.Compare(SceneId x, SceneId y) {
        return x.Value - y.Value;
      }
    }

    public class EqualityComparer : IEqualityComparer<SceneId> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<SceneId>.Equals(SceneId x, SceneId y) {
        return x.Value == y.Value;
      }

      int IEqualityComparer<SceneId>.GetHashCode(SceneId x) {
        return x.Value;
      }
    }

    public readonly int Value;

    internal SceneId(int value) {
      Value = value;
    }

    public override bool Equals(object obj) {
      if (obj is SceneId) {
        return this.Value == ((SceneId)obj).Value;
      }

      return false;
    }

    public override int GetHashCode() {
      return Value;
    }

    public override string ToString() {
      return string.Format("[SceneId:{0}]", Value);
    }

	public void Pack(UdpKit.UdpStream stream, int bits) {
		stream.WriteInt(Value, bits);
	}
	
	public void Pack(UdpKit.UdpStream stream) {
		stream.WriteInt(Value);
	}

	public static SceneId Read(UdpKit.UdpStream stream, int bits) {
		return new SceneId(stream.ReadInt(bits));
	}
	
	public static SceneId Read(UdpKit.UdpStream stream) {
		return new SceneId(stream.ReadInt());
	}

    public static bool operator ==(SceneId a, SceneId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(SceneId a, SceneId b) {
      return a.Value != b.Value;
    }
  }
}
