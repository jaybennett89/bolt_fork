using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      UdpLog.SetWriter((_, msg) => Console.WriteLine(msg));

      UdpSocket socket = new UdpSocket(new DotNetPlatform());
      socket.Start(new UdpEndPoint(UdpIPv4Address.Parse("192.168.2.173"), 0), UdpSocketMode.Host);
      socket.MasterServerConnect(new UdpEndPoint(UdpIPv4Address.Parse("94.247.169.158"), 25000));
      socket.MasterServerRequestSessionList();
      socket.SetHostInfo("Test", new byte[0]);

      Console.ReadLine();
    }
  }
}
