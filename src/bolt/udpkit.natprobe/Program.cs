#define LOCALTEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      UdpLog.SetWriter((lvl, msg) => Console.WriteLine(msg));

      Nat.Probe.Server probe = new Nat.Probe.Server(new DotNetPlatform());
      Nat.Probe.Config config = new Nat.Probe.Config();

      config.Servers[0] = UdpEndPoint.Parse("46.21.108.63:24345");
      config.Servers[1] = UdpEndPoint.Parse("94.247.169.158:28976");
      config.Servers[2] = UdpEndPoint.Parse("109.74.3.234:21593");

      config.PacketCount = 10;

      probe.Start(config);

      Console.ReadLine();
    }
  }
}
