using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  public abstract class MasterThread : UdpThread {
    public MasterConfig Config {
      get { return (MasterConfig)base.Object; }
    }

    internal MasterThread(UdpPlatform platform)
      : base(platform) {
    }
  }
}
