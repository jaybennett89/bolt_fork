using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      UdpLog.SetWriter((l, m) => Console.WriteLine(m));

      UdpSocket socket = new UdpSocket(new Guid("2098810B-9537-448A-961D-8F803D988EF2"), new DotNetPlatform());
      socket.Start(UdpEndPoint.Any, UdpSocketMode.Host);
      socket.MasterServerConnect(new UdpEndPoint(UdpIPv4Address.Parse("79.99.6.136"), 24000));
      socket.SetHostInfo("TESTHOST", null);

      Console.ReadLine();
    }
  }
}
