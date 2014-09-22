using UdpKit;

namespace Bolt {


  public struct StateFlags {
    public static readonly StateFlags ZERO = new StateFlags(0);
    public static readonly StateFlags CONTROLLING = new StateFlags(1);
    public static readonly StateFlags PROXY = new StateFlags(2);
    public static readonly StateFlags PROXYING_DISABLED = new StateFlags(4);
    public static readonly StateFlags PERSIST_ON_LOAD = new StateFlags(8);

    readonly int bits;

    public bool IsZero {
      get { return bits == 0; }
    }

    StateFlags(int val) {
      bits = val;
    }

    public override int GetHashCode() {
      return bits;
    }

    public override bool Equals(object obj) {
      if (obj is StateFlags) {
        return bits == ((StateFlags)obj).bits;
      }

      return false;
    }

    public override string ToString() {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      sb.Append("[");
      sb.Append("StateFlags");


      if ((bits & 1) == 1) {
        sb.Append(" CONTROLLING");
      }
      if ((bits & 2) == 2) {
        sb.Append(" PROXY");
      }
      if ((bits & 4) == 4) {
        sb.Append(" PROXYING_DISABLED");
      }
      if ((bits & 8) == 8) {
        sb.Append(" PERSIST_ON_LOAD");
      }

      sb.Append("]");
      return sb.ToString();
    }

    public static implicit operator bool(StateFlags a) {
      return a.bits != 0;
    }

    public static explicit operator int(StateFlags a) {
      return a.bits;
    }

    public static explicit operator StateFlags(int a) {
      return new StateFlags(a);
    }

    public static StateFlags operator &(StateFlags a, StateFlags b) {
      return new StateFlags(a.bits & b.bits);
    }

    public static StateFlags operator |(StateFlags a, StateFlags b) {
      return new StateFlags(a.bits | b.bits);
    }

    public static StateFlags operator ^(StateFlags a, StateFlags b) {
      return new StateFlags(a.bits ^ b.bits);
    }

    public static StateFlags operator ~(StateFlags a) {
      return new StateFlags(~a.bits);
    }

    public static bool operator ==(StateFlags a, StateFlags b) {
      return a.bits == b.bits;
    }

    public static bool operator !=(StateFlags a, StateFlags b) {
      return a.bits != b.bits;
    }

    //public void Pack (UdpStream stream, int bitCount) {
    //  stream.WriteInt(bits, bitCount);
    //}
    //
    //public static StateFlags Read (UdpStream stream, int bitCount) {
    //  return new StateFlags(stream.ReadInt(bitCount));
    //}
  }


}