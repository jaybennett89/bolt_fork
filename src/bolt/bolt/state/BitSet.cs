using System;

namespace Bolt {
  public struct BitSet {
    ulong Bits0;
    ulong Bits1;
    ulong Bits2;
    ulong Bits3;

    public bool IsZero {
      get {
        return 
          (Bits0 == 0UL) && 
          (Bits1 == 0UL) && 
          (Bits2 == 0UL) && 
          (Bits3 == 0UL);
      }
    }

    public BitSet Set(int bit) {
      BitSet self = this;

      switch (bit / 64) {
        case 0: self.Bits0 |= (1UL << (bit % 64)); break;
        case 1: self.Bits1 |= (1UL << (bit % 64)); break;
        case 2: self.Bits2 |= (1UL << (bit % 64)); break;
        case 3: self.Bits3 |= (1UL << (bit % 64)); break;
        default:
          throw new IndexOutOfRangeException();
      }

      return self;
    }

    public BitSet Clear(int bit) {
      BitSet self = this;

      switch (bit / 64) {
        case 0: self.Bits0 &= ~(1UL << (bit % 64)); break;
        case 1: self.Bits1 &= ~(1UL << (bit % 64)); break;
        case 2: self.Bits2 &= ~(1UL << (bit % 64)); break;
        case 3: self.Bits3 &= ~(1UL << (bit % 64)); break;
        default:
          throw new IndexOutOfRangeException();
      }

      return self;
    }

    public BitSet Combine(BitSet other) {
      BitSet self = this;

      self.Bits0 |= other.Bits0;
      self.Bits1 |= other.Bits1;
      self.Bits2 |= other.Bits2;
      self.Bits3 |= other.Bits3;

      return self;
    }

    public bool IsSet(int bit) {
      ulong b = 1UL << (bit % 64);

      switch (bit / 64) {
        case 0: return (Bits0 & b) == b;
        case 1: return (Bits1 & b) == b;
        case 2: return (Bits2 & b) == b;
        case 3: return (Bits3 & b) == b;
        default:
          throw new IndexOutOfRangeException();
      }
    }
  }
}
