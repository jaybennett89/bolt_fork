using System;
using System.Collections.Generic;
using System.Text;
using UdpKit;
using UnityEngine;

public static class BoltUtils {
  static StringBuilder concatBuilder = new StringBuilder(1024);

  public static BoltConnection GetBoltConnection (this UdpConnection self) {
    return (BoltConnection) self.UserToken;
  }

  public static string ConcatPathName(List<string> path, int limit) {
    if (limit == 1) {
      return path[0];
    }

    if (concatBuilder.Length > 0) {
      concatBuilder.Remove(0, concatBuilder.Length);
    }

    for (int i = 0; i < limit; ++i) {
      concatBuilder.Append(path[i]);

      if ((i + 1) != limit) {
        concatBuilder.Append('.');
      }
    }

    return concatBuilder.ToString();
  }

  public static T[] CloneArray<T>(this T[] array) {
    T[] clone = new T[array.Length];
    Array.Copy(array, 0, clone, 0, array.Length);
    return clone;
  }
}
