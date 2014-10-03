using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  struct PropertyMetaData {
    public int ByteOffset;
    public int ByteLength;

    public int Priority;
    public int ObjectOffset;

    public String PropertyPath;
    public String[] CallbackPaths;
    public Int32[] CallbackIndices;
  }

  abstract class PropertySerializer {
    public readonly PropertyMetaData MetaData;

    protected PropertySerializer(PropertyMetaData md) {
      MetaData = md;
      MetaData.Priority = UE.Mathf.Max(1, MetaData.Priority);
    }

    public abstract int CalculateBits(State state, State.Frame frame);
    public abstract void Pack(State state, State.Frame frame, BoltConnection connection, UdpStream stream);
    public abstract void Read(State state, State.Frame frame, BoltConnection connection, UdpStream stream);

    public virtual void OnInit(State state) { }
    public virtual void OnSimulateBefore(State state) { }
    public virtual void OnSimulateAfter(State state) { }
    public virtual void OnRender(State state, State.Frame frame) { }
    public virtual void OnChanged(State state, State.Frame frame) { }
  }
}
