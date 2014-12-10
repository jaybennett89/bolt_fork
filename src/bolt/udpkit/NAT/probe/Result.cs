using System;

namespace UdpKit.NAT.Probe {
  [Flags]
  public enum Result {
    Unknown = 0,
    EndPointPreservation = 1 << 1,
    HairpinTranslation = 1 << 2,
    AllowsUnsolicitedTraffic = 1 << 3,
    Failed = 1 << 4,
  }
}
