using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    public const uint NATPROBE_TIMEOUT = 10000;
    public const uint NATPROBE_TIMEOUT_SHORT = 500;

    class NatProbeInfo {
      public Protocol.Peer Hairpin;

      public uint Timeout;
      public uint LastSend;

      public NAT.Probe.Result Result;

      public UdpEndPoint[] Probes;
      public UdpEndPoint[] WanEndPoints = new UdpEndPoint[3];
    }

    NatProbeInfo NatProbe;

    void NatProbe_SetupProtocol() {
      protocol.SetHandler<Protocol.NatProbe_TestUnsolicited>(NatProbe_UnsolicitedTest);
      protocol.SetHandler<Protocol.NatProbe_TestHairpin>(NatProbe_HairpinTest);

      protocol.SetCallback<Protocol.NatProbe_TestEndPoint>(NatProbe_Query);
    }

    void NatProbe_Start(UdpEndPoint probe0, UdpEndPoint probe1, UdpEndPoint probe2) {
      NatProbe_Reset();

      NatProbe = new NatProbeInfo();
      NatProbe.Timeout = platform.GetPrecisionTime() + NATPROBE_TIMEOUT;
      NatProbe.Result = NAT.Probe.Result.Unknown;
      NatProbe.Probes = new UdpEndPoint[] { probe0, probe1, probe2 };
      NatProbe.Hairpin = new Protocol.Peer(platform.CreateSocket(UdpEndPoint.Any));

      protocol.Send<Protocol.NatProbe_TestEndPoint>(NatProbe.Probes[0]);
      protocol.Send<Protocol.NatProbe_TestEndPoint>(NatProbe.Probes[1]);
    }

    bool NatProbe_IsRunning() {
      return NatProbe != null;
    }

    void NatProbe_Update(uint now) {
      if (NatProbe != null) {
        if (NatProbe.Timeout < now) {
          NatProbe_Stop();
        }
      }
    }

    void NatProbe_Stop() {
      //if (NatProbe != null) {
      //  Session.Local.NatProbe_Result = NatProbe.Result;

      //  NatProbe.Hairpin.Socket.Close();
      //  NatProbe.Hairpin = null;

      //  NatProbe = null;

      //  UdpLog.Debug("NatProbe Result: {0}", Session.Local.NatProbe_Result);
      //}
    }

    void NatProbe_Reset() {
      NatProbe_Stop();

      // reset to in-progress
      //ession.Local.NatProbe_Result = NAT.Probe.Result.Unknown;
    }

    void NatProbe_HairpinTest(Protocol.NatProbe_TestHairpin msg) {
      NatProbe.Result |= NAT.Probe.Result.HairpinTranslation;
    }

    void NatProbe_UnsolicitedTest(Protocol.NatProbe_TestUnsolicited msg) {
      if (msg.Sender == NatProbe.Probes[NAT.Probe.Server.PROBE2]) {
        NatProbe.Result |= NAT.Probe.Result.AllowsUnsolicitedTraffic;
      }
    }

    void NatProbe_Query(Protocol.NatProbe_TestEndPoint query) {
      if (query.Result == null) {
        NatProbe.Result = NAT.Probe.Result.Failed;
        NatProbe_Stop();
      }
      else {
        // send hairpin test
        NatProbe.Hairpin.Send<Protocol.NatProbe_TestHairpin>(query.Result.ClientWanEndPoint);

        // store probes test
        NatProbe.WanEndPoints[query.Result.Probe] = query.Result.ClientWanEndPoint;

        if (NatProbe.WanEndPoints[0] == UdpEndPoint.Any) { return; }
        if (NatProbe.WanEndPoints[1] == UdpEndPoint.Any) { return; }

        if (NatProbe.WanEndPoints[0] == NatProbe.WanEndPoints[1]) {
          NatProbe.Result = NAT.Probe.Result.EndPointPreservation | NatProbe.Result;
        }
        else {
          NatProbe.Result = NAT.Probe.Result.Failed;
          NatProbe_Stop();
        }

        NatProbe.Timeout = GetCurrentTime() + NATPROBE_TIMEOUT_SHORT;
      }
    }
  }
}
