using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  struct EventPropertyMetaData {
    public int ByteOffset;
    public int ByteLength;
    public String PropertyName;
  }

  struct StatePropertyMetaData {
    public int ByteOffset;
    public int ByteLength;

    public int Priority;
    public int ObjectOffset;

    public bool Mecanim;
    public float MecanimDamping;

    public String PropertyName;
    public String PropertyPath;
    public String[] CallbackPaths;
    public Int32[] CallbackIndices;
  }

  abstract class PropertySerializer {
    public readonly EventPropertyMetaData EventData;
    public readonly StatePropertyMetaData StateData;

    protected PropertySerializer(StatePropertyMetaData stateData) {
      StateData = stateData;
      StateData.Priority = UE.Mathf.Max(1, StateData.Priority);
    }

    protected PropertySerializer(EventPropertyMetaData eventData) {
      EventData = eventData;
    }

    public virtual int StateBits(State state, State.Frame frame) { throw new NotSupportedException(); }
    public virtual bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual int EventBits(Event data) { throw new NotSupportedException(); }
    public virtual bool EventPack(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void EventRead(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual int CommandsBits(Command data) { throw new NotSupportedException(); }
    public virtual bool EventPack(Command data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void EventRead(Command data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual void OnInit(State state) { }
    public virtual void OnSimulateBefore(State state) { }
    public virtual void OnSimulateAfter(State state) { }
    public virtual void OnRender(State state, State.Frame frame) { }
    public virtual void OnChanged(State state, State.Frame frame) { }
  }
}
