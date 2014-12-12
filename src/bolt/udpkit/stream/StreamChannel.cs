using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UdpKit {
  class UdpStreamChannel {
    public UdpChannelName Name {
      get { return Config.ChannelName; }
    }

    public UdpChannelConfig Config;
  }
}
