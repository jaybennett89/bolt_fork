using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  public abstract class PropertySerializer {
    public readonly int Offset;
    public readonly int Length;
    public readonly int Priority;

    protected PropertySerializer(int offset, int length, int priority) {
      Offset = offset;
      Length = length;
      Priority = priority;
    }

    public abstract int CalculateBits(byte[] data);

    public abstract void Pack(int frame, UdpConnection connection, UdpStream stream, byte[] data);
    public abstract void Read(int frame, UdpConnection connection, UdpStream stream, byte[] data);
  }
}
