using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UdpKit.NAT {
  public abstract class Peer<TConfig> : UdpThread {
    public TConfig Config {
      get { return (TConfig)base.Object; }
    }

    internal Peer(UdpPlatform platform)
      : base(platform) {

    }
  }
}
