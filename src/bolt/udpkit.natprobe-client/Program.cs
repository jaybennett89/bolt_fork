using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class Program {
    public static int BitsRequired(int number) {
      if (number < 0) {
        return 32;
      }

      if (number == 0) {
        return 1;
      }

      for (int i = 31; i >= 0; --i) {
        int b = 1 << i;

        if ((number & b) == b) {
          return i + 1;
        }
      }

      throw new Exception();
    }

    static void Main(string[] args) {
      UdpLog.SetWriter((lvl, msg) => Console.WriteLine(msg));

      var probe = new Nat.Probe.Client(new DotNetPlatform());
      var config = new Nat.Probe.Config();
      config.Servers[0] = UdpEndPoint.Parse("46.21.108.63:24345");
      config.Servers[1] = UdpEndPoint.Parse("94.247.169.158:28976");
      config.Servers[2] = UdpEndPoint.Parse("109.74.3.234:21593");
      config.PacketCount = 10;

      probe.Start(config);

      if (probe.Done.WaitOne()) {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("NAT probing done.");
        Console.WriteLine("Your NAT supports the following features: {0}", probe.Result);
      }

      Console.Write("Press [Enter] to exit ...");
      Console.ReadLine();
    }
  }
}
