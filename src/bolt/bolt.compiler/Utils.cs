﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  public static class Utils {
    public static string Join<T>(this IEnumerable<T> items, string seperator) {
      return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
    }

    public static T[] Add<T>(this T[] array, T item) {
      if (array == null) {
        return new T[1] { item };
      }

      T[] clone = new T[array.Length + 1];
      array.CopyTo(clone, 0);
      clone[array.Length] = item;
      return clone;
    }
  }
}
