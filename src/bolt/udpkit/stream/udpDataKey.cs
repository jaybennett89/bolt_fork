using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public struct UdpDataKey {
    public class Comparer : IComparer<UdpDataKey> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<UdpDataKey>.Compare(UdpDataKey x, UdpDataKey y) {
        return x.Guid.CompareTo(y.Guid);
      }
    }

    public class EqualityComparer : IEqualityComparer<UdpDataKey> {
      public static readonly EqualityComparer Instance = new EqualityComparer();

      EqualityComparer() {

      }

      bool IEqualityComparer<UdpDataKey>.Equals(UdpDataKey x, UdpDataKey y) {
        return x.Guid == y.Guid;
      }

      int IEqualityComparer<UdpDataKey>.GetHashCode(UdpDataKey obj) {
        return obj.Guid.GetHashCode();
      }
    }

    internal Guid Guid;

    public bool IsZero {
      get { return Guid == Guid.Empty; }
    }

    internal UdpDataKey(Guid guid) {
      Guid = guid;
    }

    public override int GetHashCode() {
      return Guid.GetHashCode();
    }

    public override bool Equals(object obj) {
      if (obj is UdpDataKey) {
        return this.Guid == ((UdpDataKey)obj).Guid;
      }

      return false;
    }

    public override string ToString() {
      return string.Format("[UdpChannelDataKey {0}]", Guid);
    }

    public static bool operator ==(UdpDataKey l, UdpDataKey r) {
      return l.Guid == r.Guid;
    }

    public static bool operator !=(UdpDataKey l, UdpDataKey r) {
      return l.Guid != r.Guid;
    }

    internal static UdpDataKey Generate() {
      return new UdpDataKey(Guid.NewGuid());
    }
  }
}
