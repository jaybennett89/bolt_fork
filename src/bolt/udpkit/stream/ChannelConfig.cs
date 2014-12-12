using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using System.Text;

namespace UdpKit {
  public enum UdpChannelMode {
    Unreliable = 0,
    Reliable = 1
  }

  internal class UdpChannelConfig {
    public int Priority;
    public UdpChannelMode Mode;
    public UdpChannelName ChannelName;
  }
}
