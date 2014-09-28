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

    protected PropertySerializer(int byteOffset, int byteLength, int objectOffset, int priority) {
      ByteOffset = byteOffset;
      ByteLength = byteLength;
      ObjectOffset = objectOffset;
      Priority = Math.Max(1, priority);
    }

    public virtual void Changed(State state) {

    }

    public abstract int CalculateBits(byte[] data);
    public abstract void Pack(State.Frame frame, BoltConnection connection, UdpStream stream);
    public abstract void Read(State.Frame frame, BoltConnection connection, UdpStream stream);
  }
}
