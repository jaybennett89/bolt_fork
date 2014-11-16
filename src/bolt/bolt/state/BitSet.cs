using System;

namespace Bolt {
  public class BitSet {
    bool zero;

    readonly int size;
    readonly ulong[] bits;

    public int Size {
      get { return size; }
    }

    public bool Zero {
      get { return zero; }
    }

    BitSet(int size, ulong[] bits) {
      Assert.True(size <= (bits.Length * 64));

      this.size = size;
      this.bits = bits;
      this.zero = false;
    }

    BitSet(int size) {
      int length = size / 64;

      if ((size % 64) > 0) {
        length += 1;
      }

      this.zero = true;
      this.size = size;
      this.bits = new ulong[length];
    }

    public void Set(int bit) {
      Assert.True(bit < this.size);
      this.zero = false;
      this.bits[bit / 64] |= (1UL << (bit % 64));
    }

    public void Clear(int bit) {
      Assert.True(bit < this.size);
      this.bits[bit / 64] &= ~(1UL << (bit % 64));
    }

    public bool IsSet(int bit) {
      Assert.True(bit < this.size);
      return (bits[bit / 64] & (1UL << (bit % 64))) != 0UL;
    }

    public bool IsClear(int bit) {
      Assert.True(bit < this.size);
      return (bits[bit / 64] & (1UL << (bit % 64))) == 0UL;
    }

    public void Merge(BitSet set) {
      Assert.True(this.size == set.size);
      Assert.True(this.bits.Length == set.bits.Length);

      for (int i = 0; i < this.bits.Length; ++i) {
        this.bits[i] |= set.bits[i];
      }
    }

    public void ClearAll() {
      // clear entire array
      Array.Clear(this.bits, 0, this.bits.Length);

      // set zero state
      this.zero = true;
    }

    public static BitSet Create(int size, ulong[] bits) {
      return new BitSet(size, bits);
    }

    public static BitSet Create(int size) {
      return new BitSet(size);
    }

    public static BitSet CreateSet(int size) {
      BitSet set = Create(size);

      for (int i = 0; i < size; ++i) {
        set.Set(i);
      }

      return set;
    }
  }
}
