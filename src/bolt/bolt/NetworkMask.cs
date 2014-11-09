using System;

namespace Bolt {
  internal struct NetworkMask {
    ulong Bits0;
    ulong Bits1;

    public bool IsZero {
      get { return (Bits0 == 0UL) && (Bits1 == 0UL); }
    }

    public NetworkMask Set(int bit) {
      NetworkMask self = this;

      switch (bit / 64) {
        case 0: self.Bits0 |= (1UL << (bit % 64)); break;
        case 1: self.Bits1 |= (1UL << (bit % 64)); break;
        default: throw new IndexOutOfRangeException();
      }

      return self;
    }

    public NetworkMask Clear(int bit) {
      NetworkMask self = this;

      switch (bit / 64) {
        case 0: self.Bits0 &= ~(1UL << (bit % 64)); break;
        case 1: self.Bits1 &= ~(1UL << (bit % 64)); break;
        default: throw new IndexOutOfRangeException();
      }

      return self;
    }

    public NetworkMask Combine(NetworkMask other) {
      NetworkMask self = this;

      self.Bits0 |= other.Bits0;
      self.Bits1 |= other.Bits1;

      return self;
    }

    public bool IsSet(int bit) {
      ulong b = 1UL << (bit % 64);

      switch (bit / 64) {
        case 0: return (Bits0 & b) == b;
        case 1: return (Bits1 & b) == b;
        default: throw new IndexOutOfRangeException();
      }
    }
  }
}
