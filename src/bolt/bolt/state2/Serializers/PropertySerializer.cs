using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal struct PropertyMetaData {
    public int ByteOffset;
    public int ByteLength;

    public int Priority;
    public int ObjectOffset;

    public String PropertyPath;
    public String[] CallbackPaths;
    public Int32[] CallbackIndices;
  }

  internal abstract class PropertySerializer {
    public readonly PropertyMetaData MetaData;

    protected PropertySerializer(PropertyMetaData md) {
      MetaData = md;
      MetaData.Priority = UE.Mathf.Max(1, MetaData.Priority);
    }

    public abstract int CalculateBits(byte[] data);
    public abstract void Pack(State.Frame frame, BoltConnection connection, UdpStream stream);
    public abstract void Read(State.Frame frame, BoltConnection connection, UdpStream stream);
  }
}
