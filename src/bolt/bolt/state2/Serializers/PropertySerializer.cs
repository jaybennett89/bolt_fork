using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal abstract class PropertySerializer {
    public readonly int ByteOffset;
    public readonly int ByteLength;
    public readonly int ObjectOffset;
    public readonly int Priority;

    protected PropertySerializer(int offset, int length, int objectOffset, int priority) {
      ByteOffset = offset;
      ByteLength = length;
      ObjectOffset = objectOffset;
      Priority = priority;
    }

    public abstract int CalculateBits(byte[] data);

    public abstract void Pack(State.Frame frame, BoltConnection connection, UdpStream stream);
    public abstract void Read(State.Frame frame, BoltConnection connection, UdpStream stream);
  }
}
