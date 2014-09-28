using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  internal struct PropertyMetaData {
    public int ByteOffset;
    public int ByteLength;

    public int Priority;
    public int ObjectOffset;

    public String  CallbackPath;
    public Int32[] CallbackIndices;
  }

  internal abstract class PropertySerializer {
    public readonly PropertyMetaData Data;

    protected PropertySerializer(PropertyMetaData data) {
      Data = data;
    }

    public virtual void Changed(State state) {

    }

    public abstract int CalculateBits(byte[] data);
    public abstract void Pack(State.Frame frame, BoltConnection connection, UdpStream stream);
    public abstract void Read(State.Frame frame, BoltConnection connection, UdpStream stream);
  }
}
