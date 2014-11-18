using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public abstract class ValueArray<T> {
    internal Bolt.State state;

    internal int length;
    internal int offsetStorage;
    internal int offsetObjects;

    internal ValueArray(Bolt.State state, int offsetStorage, int offsetObjects, int length) {
      this.state = state;

      this.length = length;
      this.offsetStorage = offsetStorage;
      this.offsetObjects = offsetObjects;
    }

    public int Length {
      get {
        return length;
      }
    }

    public T this[int index] {
      get {
        if (index < 0 || index >= length) {
          throw new IndexOutOfRangeException();
        }

        return GetValue(offsetStorage + index);
      }
      set {
        if (index < 0 || index >= length) {
          throw new IndexOutOfRangeException();
        }

        SetValue(index, value);
      }
    }

    protected abstract T GetValue(int index);
    protected abstract void SetValue(int index, T value);
  }
}
