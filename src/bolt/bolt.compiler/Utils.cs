using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public static class Utils {
    public static string Join<T>(this IEnumerable<T> items, string seperator) {
      return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
    }
  }
}
