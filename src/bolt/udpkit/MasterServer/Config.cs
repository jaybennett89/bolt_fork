using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.MasterServer {
  public class Config {
    public UdpEndPoint Master;

    public UdpEndPoint Probe0;
    public UdpEndPoint Probe1;
    public UdpEndPoint Probe2;

    public UdpEndPoint Punch;
  }
}
