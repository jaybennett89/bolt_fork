﻿using System;

namespace Bolt {
  public class BitArray {
    readonly int size;
    readonly int[] bits;

    public int Size {
      get { return size; }
    }

    BitArray(int size, int[] bits) {
      Assert.True(size <= (bits.Length * 32));

      this.size = size;
      this.bits = bits;
    }

    BitArray(int size) {
      int length = size / 32;

      if ((size % 32) > 0) {
        length += 1;
      }

      this.size = size;
      this.bits = new int[length];
    }

    public void Clear() {
      Array.Clear(bits, 0, bits.Length);
    }

    public int[] ToArray() {
      int[] clone = new int[bits.Length];
      Array.Copy(bits, 0, clone, 0, bits.Length);
      return clone;
    }

    public void Set(int bit) {
      bits[bit / 32] |= (1 << (bit % 32));
    }

    public void Clear(int bit) {
      bits[bit / 32] &= ~(1 << (bit % 32));
    }

    public void OrAssign(BitArray that) {
      Assert.True(this.size == that.size);

      for (int i = 0; i < that.bits.Length; ++i) {
        this.bits[i] |= that.bits[i];
      }
    }

    public static BitArray CreateFrom(int size, int[] bits) {
      return new BitArray(size, bits);
    }

    public static BitArray CreateClear(int size) {
      return new BitArray(size);
    }

    public static BitArray CreateSet(int size) {
      BitArray array = CreateClear(size);

      for (int i = 0; i < size; ++i) {
        array.Set(i);
      }

      return array;
    }
  }
}
