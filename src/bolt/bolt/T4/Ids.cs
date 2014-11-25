using System.Collections.Generic;

static class IdExtensions {
	public static void WritePrefabId(this UdpKit.UdpPacket stream, Bolt.PrefabId id, int bits) {
		stream.WriteInt(id.Value, bits);
	}
	
	public static void WritePrefabId(this UdpKit.UdpPacket stream, Bolt.PrefabId id) {
		stream.WritePrefabId(id, 32);
	}

	public static Bolt.PrefabId ReadPrefabId(this UdpKit.UdpPacket stream, int bits) {
		return new Bolt.PrefabId(stream.ReadInt(bits));
	}

	public static Bolt.PrefabId ReadPrefabId(this UdpKit.UdpPacket stream) {
		return stream.ReadPrefabId(32);
	}
	public static void WriteTypeId(this UdpKit.UdpPacket stream, Bolt.TypeId id, int bits) {
		stream.WriteInt(id.Value, bits);
	}
	
	public static void WriteTypeId(this UdpKit.UdpPacket stream, Bolt.TypeId id) {
		stream.WriteTypeId(id, 32);
	}

	public static Bolt.TypeId ReadTypeId(this UdpKit.UdpPacket stream, int bits) {
		return new Bolt.TypeId(stream.ReadInt(bits));
	}

	public static Bolt.TypeId ReadTypeId(this UdpKit.UdpPacket stream) {
		return stream.ReadTypeId(32);
	}
}

namespace Bolt {
[Documentation]
[System.Serializable]
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

    public int Value;

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
[System.Serializable]
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

    public int Value;

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

    public static bool operator ==(TypeId a, TypeId b) {
      return a.Value == b.Value;
    }

    public static bool operator !=(TypeId a, TypeId b) {
      return a.Value != b.Value;
    }
  }
}
