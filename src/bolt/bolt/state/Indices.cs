using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public struct ArrayIndices {
    readonly bool hasValue;
    readonly int index;

    internal ArrayIndices(int index) {
      this.index = index;
      this.hasValue = true;
    }

    public int Length {
      get { return hasValue ? index : -1; }
    }

    public int this[int index] {
      get {
        if (index < 0 || index > 0 || !hasValue) {
          throw new IndexOutOfRangeException();
        }

        return index;
      }
    }
  }
}
