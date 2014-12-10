using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      UdpLog.SetWriter((_, msg) => Console.WriteLine(msg));

      UdpSocket socket;
      socket = new UdpSocket(new DotNetPlatform());
      socket.Start(new UdpEndPoint(UdpIPv4Address.Parse("192.168.1.100"), 0), UdpSocketMode.Client);
      socket.MasterServerSet(new UdpEndPoint(UdpIPv4Address.Parse("94.247.169.158"), 25000));

      bool connecting = false;
      DateTime nextRequest = DateTime.MinValue;

      while (true) {
        UdpEvent ev;

        while (socket.Poll(out ev)) {
          switch (ev.EventType) {
            case UdpEventType.SessionListUpdated:
              if (!connecting) {
                connecting = true;
                socket.Connect(ev.SessionList.First().Value);
              }
              break;
          }
        }

        if ((connecting == false) && (nextRequest < DateTime.Now)) {
          socket.MasterServerRequestSessionList();
          nextRequest = DateTime.Now.AddSeconds(1);
        }

        Thread.Sleep(1);
      }
    }
  }
}
