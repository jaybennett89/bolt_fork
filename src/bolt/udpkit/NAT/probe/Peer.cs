using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UdpKit.NAT.Probe {
  public abstract class Peer :  {
    public const byte PROBE_COUNT = 3;

    public const byte PROBE0 = 0;
    public const byte PROBE1 = 1;
    public const byte PROBE2 = 2;

    public const byte CLIENT_COUNT = 2;

    public const byte CLIENT0 = 128;
    public const byte CLIENT1 = 129;

    protected new NAT.Probe.Config Config;

    protected Peer(UdpPlatform platform)
      : base(platform) {

    }

    public override void Start(object config) {
      Config = (NAT.Probe.Config)config;
      base.Start(config);
    }
  }
}
