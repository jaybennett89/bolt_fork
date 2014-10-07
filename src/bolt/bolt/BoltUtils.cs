using System;
using System.Text.RegularExpressions;
using UdpKit;
using UnityEngine;

public static class BoltUtils {
  public static bool IsCSharpIdentifier (string a) {
    return Regex.IsMatch(a, "^[a-zA-Z_][a-zA-Z0-9_]*$");
  }

  public static bool StringEquals (string a, string b) {
    if (a == null && b == null)
      return true;

    if (a == null)
      return false;

    if (b == null)
      return false;

    return a.Equals(b);
  }

  public static BoltConnection GetBoltConnection (this UdpConnection self) {
    return (BoltConnection) self.UserToken;
  }

  public static T[] CloneArray<T>(this T[] array) {
    T[] clone = new T[array.Length];
    Array.Copy(array, 0, clone, 0, array.Length);
    return clone;
  }
}
