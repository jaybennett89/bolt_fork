using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UdpKit;
using UnityEngine;

static class BoltUtils {
  public static BoltConnection GetBoltConnection (this UdpConnection self) {
    return (BoltConnection) self.UserToken;
  }

  public static string Join<T>(this IEnumerable<T> items, string seperator) {
    return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
  }

  public static bool ViewPointIsOnScreen(this Vector3 vp) {
    return vp.z >= 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
  }

  public static T[] CloneArray<T>(this T[] array) {
    T[] clone = new T[array.Length];
    Array.Copy(array, 0, clone, 0, array.Length);
    return clone;
  }

  public static T[] AddFirst<T>(this T[] array, T item) {
    if (array == null) {
      return new T[1] { item };
    }

    // duplicate + 1 extra slot
    T[] clone = new T[array.Length + 1];

    // copy old items to index 1 ... n
    Array.Copy(array, 0, clone, 1, array.Length);

    // insert new item at index 0
    clone[0] = item;

    return clone;
  }
}
