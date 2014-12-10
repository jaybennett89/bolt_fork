using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using System.Text;

namespace UdpKit {
  public enum UdpChannelMode {
    Unreliable = 0,
    Reliable = 1
  }

  [StructLayout(LayoutKind.Explicit)]
  internal struct UdpChannelConfig {
    [FieldOffset(0)]
    public int Priority;

    [FieldOffset(4)]
    public UdpChannelMode Mode;

    [FieldOffset(8)]
    public UdpChannelName ChannelName;
  }
}
