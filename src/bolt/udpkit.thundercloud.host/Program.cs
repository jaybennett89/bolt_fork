﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpKit {
  class Program {
    static void Main(string[] args) {
      var n = DateTime.Now;
      var fmt = "Zeus_HOSTTEST_Log_{0}Y{1}M{2}D_{3}H{4}M{5}S_{6}MS.txt";
      var logFile = String.Format(fmt, n.Year, n.Month, n.Day, n.Hour, n.Minute, n.Second, n.Millisecond);

      var log = File.OpenWrite(logFile);
      var writer = new StreamWriter(log);

      UdpLog.SetWriter((l, m) => {
        lock (log) {
          writer.WriteLine(m);
          writer.Flush();

          Console.WriteLine(m);
        }
      });

      UdpSocket socket = new UdpSocket(new Guid("2098810B-9537-448A-961D-8F803D988EF2"), new DotNetPlatform());
      socket.Start(UdpEndPoint.Any, null, UdpSocketMode.Host);
      socket.MasterServerConnect(new UdpEndPoint(UdpIPv4Address.Parse("79.99.6.136"), 24000));
      socket.SetHostInfo("TESTHOST", true, null);


      while (true) {
        UdpEvent ev;

        while (socket.Poll(out ev)) {
          UdpLog.Info(ev.EventType.ToString());

          switch (ev.EventType) {
            case UdpEventType.MasterServerNatProbeResult:
              UdpLog.Info("UdpEventType.MasterServerNatProbeResult: " + ev.NatFeatures);
              break;
          }
        }

        Thread.Sleep(1);
      }

      Console.ReadLine();
    }
  }
}
