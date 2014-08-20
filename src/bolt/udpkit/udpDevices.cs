using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  public enum UdpDevices : int {
    Unknown = 0,
    WebPlayer = 1,

    OSXDesktop = 100,
    LinuxDesktop = 101,
    WindowsDesktop = 102,

    iOS = 200,
    Android = 201,
  }
}
