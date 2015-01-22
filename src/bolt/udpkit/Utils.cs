using System;
using System.Linq;
using System.Collections.Generic;

namespace UdpKit {
  internal static class UdpUtils {
    public static string Join<T>(this IEnumerable<T> items, string seperator) {
      return String.Join(seperator, items.Select(x => x.ToString()).ToArray());
    }

    public static bool HasValue(this string value) {
      if (value == null) {
        return false;
      }

      if (value.Length == 0) {
        return false;
      }

      if (value.Trim().Length == 0) {
        return false;
      }

      return true;
    }
    public static byte[] ReadToken(byte[] buffer, int size, int tokenStart) {
      byte[] token = null;

      if (size > tokenStart) {
        token = new byte[size - tokenStart];
        Buffer.BlockCopy(buffer, tokenStart, token, 0, size - tokenStart);
      }

      return token;
    }
  }
}
