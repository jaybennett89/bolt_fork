using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  struct DoubleBuffer<T> where T : struct {
    public T Value0;
    public T Value1;

    public DoubleBuffer<T> Shift(T value) {
      DoubleBuffer<T> buffer = this;
      buffer.Value0 = Value1;
      buffer.Value1 = value;
      return buffer;
    }

    public static DoubleBuffer<T> InitBuffer(T value) {
      DoubleBuffer<T> buffer;
      buffer.Value0 = value;
      buffer.Value1 = value;
      return buffer;
    }
  }
}
