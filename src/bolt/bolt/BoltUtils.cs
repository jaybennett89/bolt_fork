using System;
using System.Text.RegularExpressions;
using UdpKit;
using UnityEngine;

public static class BoltUtils {
  public static BoltConnection GetBoltConnection (this UdpConnection self) {
    return (BoltConnection) self.UserToken;
  }

  public static T[] CloneArray<T>(this T[] array) {
    T[] clone = new T[array.Length];
    Array.Copy(array, 0, clone, 0, array.Length);
    return clone;
  }
}
