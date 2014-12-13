﻿using System;

namespace UdpKit {
  internal static class UdpUtils {
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