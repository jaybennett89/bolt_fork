using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UdpKit;

namespace ConsoleApplication1 {
  class Program {

    static UdpChannelName channel;
    static byte[] data = new byte[1024 * 1024];

    [MethodImpl(MethodImplOptions.Synchronized)]
    static void Log(string msg, params object[] args) {
      Console.WriteLine(msg, args);
    }

    static void Peer(object obj) {
      UdpSocket socket = (UdpSocket)obj;

      UdpEvent ev;

      while (true) {
        while (socket.Poll(out ev)) {
          Log(ev.EventType.ToString());

          switch (ev.EventType) {
            case UdpEventType.Connected:
              ev.Connection.StreamBytes(channel, data);
              break;

            case UdpEventType.StreamDataReceived:
              Log(ev.Connection.RemoteEndPoint + ": " + Encoding.ASCII.GetString(ev.StreamData.Data));
              break;
          }
        }

        Thread.Sleep(1);
      }
    }

    static void StartThread(UdpSocket socket) {
      Thread t = new Thread(Peer);
      t.IsBackground = true;
      t.Name = "UdpKit Peer Thread";
      t.Start(socket);
    }

    static void Main(string[] args) {
      UdpLog.SetWriter((lvl, msg) => Log(msg));

      UdpConfig config = new UdpConfig();
      config.ConnectionTimeout = 1000000000;

      UdpSocket server = new UdpSocket(new UdpPlatformManaged(), config);
      channel = server.StreamChannelCreate("Text", UdpChannelMode.Reliable, 1);
      server.Start(new UdpEndPoint(UdpIPv4Address.Localhost, 40000));

      UdpSocket client = new UdpSocket(new UdpPlatformManaged(), config);
      channel = client.StreamChannelCreate("Text", UdpChannelMode.Reliable, 1);
      client.Start(UdpEndPoint.Any);
      client.Connect(new UdpEndPoint(UdpIPv4Address.Localhost, 40000));

      StartThread(server);
      StartThread(client);

      Console.ReadLine();
    }
  }
}
