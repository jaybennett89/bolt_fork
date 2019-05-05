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

    public BitSet[] Filters;
    public SerializerGroup SerializerGroup;

    public StateMetaData() {
      Filters = new BitSet[33];
      SerializerGroup = new SerializerGroup();
    }
  }

  internal abstract class NetworkState_Meta : NetworkObj_Meta {
    public int PacketMaxBits;
    public int PacketMaxProperties;
  }
}
