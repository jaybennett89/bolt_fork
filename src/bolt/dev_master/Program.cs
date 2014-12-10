using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      UdpLog.SetWriter((_, msg) => Console.WriteLine(msg));

      MasterServer.Config cfg;
      
      cfg = new MasterServer.Config();
      cfg.Master = new UdpEndPoint(UdpIPv4Address.Parse("94.247.169.158"), 25000);
      cfg.Punch = new UdpEndPoint(UdpIPv4Address.Parse("94.247.169.158"), 27000);
      cfg.Probe0 = new UdpEndPoint(UdpIPv4Address.Parse("94.247.169.158"), 26000);
      cfg.Probe1 = new UdpEndPoint(UdpIPv4Address.Parse("109.74.3.234"), 26001);
      cfg.Probe2 = new UdpEndPoint(UdpIPv4Address.Parse("46.21.108.63"), 26002);

      MasterServer.Server srv;

      srv = new MasterServer.Server(new DotNetPlatform());
      srv.Start(cfg);

      Console.ReadLine();
    }
  }
}
