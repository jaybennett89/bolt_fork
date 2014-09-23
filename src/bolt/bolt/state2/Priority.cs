using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  public struct PropertyPriority {
    public class Comparer : IComparer<PropertyPriority> {
      public static readonly Comparer Instance = new Comparer();

      Comparer() {

      }

      int IComparer<PropertyPriority>.Compare(PropertyPriority x, PropertyPriority y) {
        return y.Priority - x.Priority;
      }
    }

    public int Property;
    public int Priority;
  }
}
