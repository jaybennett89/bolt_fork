using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class StateMetaData {
    public TypeId TypeId;
    public NetworkFramePool FramePool;

    public int PacketMaxBits;
    public int PacketMaxProperties;

    public BitSet ControllerFilter;

    public SerializerGroup SerializerGroup;
    public List<PropertySerializer> SerializersOnRender;
    public List<PropertySerializer> SerializersOnSimulateAfter;
    public List<PropertySerializer> SerializersOnSimulateBefore;

    public StateMetaData() {
      SerializerGroup = new SerializerGroup();
      SerializersOnRender = new List<PropertySerializer>();
      SerializersOnSimulateAfter = new List<PropertySerializer>();
      SerializersOnSimulateBefore = new List<PropertySerializer>();
    }
  }
}
