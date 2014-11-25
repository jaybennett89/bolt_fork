using System;
using System.Runtime.InteropServices;

#if BOLT_COMPILER_DLL
namespace Bolt.Compiler {
#else
namespace Bolt {
#endif

  public struct BitSetIterator {
    int number;
    int numberBit;

    BitSet set;

    public BitSetIterator(BitSet set) {
      this.number = 0;
      this.numberBit = 0;
      this.set = set;
    }

    public bool Next(out int bit) {
      ulong bits;

      while (true) {

        switch (number) {
          case 0: bits = set.Bits0; break;
          case 1: bits = set.Bits1; break;
          case 2: bits = set.Bits2; break;
          case 3: bits = set.Bits3; break;

          case 4:
            bit = -1;
            return false;

          default:
            throw new InvalidOperationException();
        }

        if (bits == 0) {
          number = number + 1;
          numberBit = 0;
        }
        else {
          for (; numberBit < 64; ++numberBit) {
            if ((bits & (1UL << numberBit)) != 0UL) {
              switch (number) {
                case 0: set.Bits0 &= ~(1UL << numberBit); break;
                case 1: set.Bits1 &= ~(1UL << numberBit); break;
                case 2: set.Bits2 &= ~(1UL << numberBit); break;
                case 3: set.Bits3 &= ~(1UL << numberBit); break;
              }

              // set bit we found
              bit = (number * 64) + numberBit;

              // done!
              return true;
            }
          }

          throw new InvalidOperationException();
        }
      }
    }
  }

  public struct BitSet {
    internal static readonly BitSet Full;

    static BitSet() {
      Full = default(BitSet);
      Full.Bits0 = ulong.MaxValue;
      Full.Bits1 = ulong.MaxValue;
      Full.Bits2 = ulong.MaxValue;
      Full.Bits3 = ulong.MaxValue;
    }

    internal ulong Bits0;
    internal ulong Bits1;
    internal ulong Bits2;
    internal ulong Bits3;

    internal BitSet(ulong bits0, ulong bits1, ulong bits2, ulong bits3) {
      Bits0 = bits0;
      Bits1 = bits1;
      Bits2 = bits2;
      Bits3 = bits3;
    }

    public bool IsZero {
      get {
        return
          (Bits0 == 0UL) &&
          (Bits1 == 0UL) &&
          (Bits2 == 0UL) &&
          (Bits3 == 0UL);
      }
    }

    public void Set(int bit) {
      switch (bit / 64) {
        case 0: this.Bits0 |= (1UL << (bit % 64)); break;
        case 1: this.Bits1 |= (1UL << (bit % 64)); break;
        case 2: this.Bits2 |= (1UL << (bit % 64)); break;
        case 3: this.Bits3 |= (1UL << (bit % 64)); break;
        default:
          throw new IndexOutOfRangeException();
      }

      Assert.False(IsZero);
    }

    public void Clear(int bit) {
      switch (bit / 64) {
        case 0: this.Bits0 &= ~(1UL << (bit % 64)); break;
        case 1: this.Bits1 &= ~(1UL << (bit % 64)); break;
        case 2: this.Bits2 &= ~(1UL << (bit % 64)); break;
        case 3: this.Bits3 &= ~(1UL << (bit % 64)); break;
        default:
          throw new IndexOutOfRangeException();
      }
    }

    public void Combine(BitSet other) {
      this.Bits0 |= other.Bits0;
      this.Bits1 |= other.Bits1;
      this.Bits2 |= other.Bits2;
      this.Bits3 |= other.Bits3;
    }

    public void ClearAll() {
      Bits0 = 0UL;
      Bits1 = 0UL;
      Bits2 = 0UL;
      Bits3 = 0UL;
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

    public BitSetIterator GetIterator() {
      return new BitSetIterator(this);
    }

  }
}
