using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UdpKit.NAT.UPnP {
  class Client : UdpThread {
    volatile int Port;
    volatile Result result;

    public Result Result {
      get { return result; }
    }

    public Client(UdpPlatform platform, int port)
      : base(platform) {
      Port = port;
    }

    protected override void OnInit() {
      result = UPnP.Result.InProgress;
    }

    protected override void OnUpdate() {
      Thread.Sleep(1000);
      result = NAT.UPnP.Result.Failed;
      Stop();
    }
  }
}
