using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public struct Priority {
    public class Comparer : IComparer<Priority> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<Priority>.Compare(Priority x, Priority y) {
        return y.Value - x.Value;
      }
    }

    public int Property;
    public int Value;
  }
}
