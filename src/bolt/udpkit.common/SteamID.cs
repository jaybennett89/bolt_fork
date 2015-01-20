using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  public struct UdpSteamID {
    public readonly ulong Id;

    public UdpSteamID(ulong id) {
      Id = id;
    }
  }
}
