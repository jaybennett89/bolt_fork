using System;
using System.Collections.Generic;

namespace UdpKit {
  public class Wp8SocketBase {
    readonly protected internal Object syncroot = new Object();
    readonly protected internal int bufferSize;

    readonly Stack<byte[]> bufferPool = new Stack<byte[]>();

    public Wp8SocketBase(int buffSize) {
      bufferSize = buffSize;
    }

    public byte[] NewBuffer() {
      lock (syncroot) {
        if (bufferPool.Count > 0) {
          return bufferPool.Pop();
        }

        return new byte[bufferSize];
      }
    }

    public void FreeBuffer(byte[] buffer) {
      Array.Clear(buffer, 0, bufferSize);

      lock (syncroot) {
        bufferPool.Push(buffer);
      }
    }
  }
}
