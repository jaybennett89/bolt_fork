using System;

public struct Bits : IEquatable<Bits>, IComparable<Bits> {
  public static readonly Bits zero = new Bits(0);
  public static readonly Bits all = new Bits(uint.MaxValue);

  UInt32 bits;

  public int PopCount {
    get { return BoltMath.PopCount(bits); }
  }

  public Bits (uint value) {
    bits = value;
  }

  public void Set (UInt32 bits) {
    this.bits |= bits;
  }

  public void Clear (UInt32 bits) {
    this.bits &= ~bits;
  }

  public bool IsClear (UInt32 bits) {
    return (this.bits & bits) == 0;
  }

  public bool IsSet (UInt32 bits) {
    return (this.bits & bits) == bits;
  }

  public bool IsAny (UInt32 bits) {
    return (this.bits & bits) != 0;
  }

  public static implicit operator Boolean (Bits flags) {
    return flags.bits != 0;
  }

  public static implicit operator Bits (UInt32 bits) {
    Bits flags;
    flags.bits = bits;
    return flags;
  }

  public static implicit operator UInt32 (Bits bits) {
    return bits.bits;
  }

  public static bool operator == (Bits a, Bits b) {
    return a.bits == b.bits;
  }

  public static bool operator != (Bits a, Bits b) {
    return a.bits != b.bits;
  }

  public static bool operator > (Bits a, Bits b) {
    return a.bits > b.bits;
  }

  public static bool operator < (Bits a, Bits b) {
    return a.bits < b.bits;
  }

  public static bool operator >= (Bits a, Bits b) {
    return a.bits >= b.bits;
  }

  public static bool operator <= (Bits a, Bits b) {
    return a.bits <= b.bits;
  }

  public static Bits operator & (Bits a, Bits b) {
    return a.bits & b.bits;
  }

  public static Bits operator & (Bits a, UInt32 b) {
    return a.bits & b;
  }

  public static Bits operator & (UInt32 b, Bits a) {
    return a.bits & b;
  }

  public static Bits operator | (Bits a, Bits b) {
    return a.bits | b.bits;
  }

  public static Bits operator | (Bits a, UInt32 b) {
    return a.bits | b;
  }

  public static Bits operator | (UInt32 b, Bits a) {
    return a.bits | b;
  }

  public static Bits operator ^ (Bits a, Bits b) {
    return a.bits ^ b.bits;
  }

  public static Bits operator ^ (Bits a, UInt32 b) {
    return a.bits ^ b;
  }

  public static Bits operator ^ (UInt32 b, Bits a) {
    return a.bits ^ b;
  }

  public static Bits operator >> (Bits a, int shift) {
    return a.bits >> shift;
  }

  public static Bits operator << (Bits a, int shift) {
    return a.bits << shift;
  }

  public static Bits operator ~ (Bits a) {
    return ~a.bits;
  }

  public override string ToString () {
    return BitUtils.UIntToString(bits);
  }

  public override bool Equals (object other) {
    if (other is Bits) {
      return Equals((Bits) other);
    }

    return false;
  }

  public override int GetHashCode () {
    return bits.GetHashCode();
  }

  public int CompareTo (Bits other) {
    return this.bits.CompareTo(other.bits);
  }

  public bool Equals (Bits other) {
    return this.bits == other.bits;
  }
}
