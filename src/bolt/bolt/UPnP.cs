using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace BoltInternal {
  public abstract class NatCommunicator {
    public abstract IEnumerable<Bolt.INatDevice> NatDevices { get; }
    public abstract void Enable();
    public abstract void Disable(bool async);
  }
}

namespace Bolt {
  public interface IPortMapping {
    ushort External { get; }
    ushort Internal { get; }
  }

  public interface INatDevice {
    string DeviceType { get; }
    IEnumerable<IPortMapping> OpenPorts { get; }
    UdpIPv4Address PublicAddress { get; }
    UdpIPv4Address LocalAddress { get; }
  }

  static class UPnP {
    static bool Available() {
      switch (UE.Application.platform) {
        case UE.RuntimePlatform.OSXEditor:
        case UE.RuntimePlatform.OSXPlayer:
        case UE.RuntimePlatform.LinuxPlayer:
        case UE.RuntimePlatform.WindowsEditor:
        case UE.RuntimePlatform.WindowsPlayer:
          if (BoltNetworkInternal.NatCommunicator == null) {
            BoltLog.Error("Could not locate UPnP library, is it installed?");
            return false;
          }
          else {
            return true;
          }

        default:
          BoltLog.Error("UPnP is only available on desktop platforms");
          return false;
      }
    }

    static public void Enable() {
      if (Available()) {
        BoltNetworkInternal.NatCommunicator.Enable();
      }
    }

    static public void Disable(bool async) {
      if (Available()) {
        BoltNetworkInternal.NatCommunicator.Disable(async);
      }
    }
  }
}
