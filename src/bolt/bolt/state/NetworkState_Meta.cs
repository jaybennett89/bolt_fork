using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal abstract class NetworkState_Meta : NetworkObj_Meta {
    public int PropertyIdBits;

    public int PacketMaxBits;
    public int PacketMaxProperties;
    public int PacketMaxPropertiesBits;
  }
}
