using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace BoltInternal {
  public abstract class NatCommunicator {
    public abstract bool IsEnabled { get; }
    public abstract IEnumerable<Bolt.INatDevice> NatDevices { get; }
    public abstract void Enable();
    public abstract void Update();
    public abstract void Disable(bool async);
    public abstract void OpenPort(int port);
    public abstract void ClosePort(int port);
    public abstract bool NextPortStatusChange(out Bolt.INatDevice device, out Bolt.IPortMapping mapping);
  }
}

namespace Bolt {

  public enum NatPortMappingStatus {
    Closed = -1, 
    Unknown = 0, 
    Open = 1
  }

  public interface IPortMapping {
    ushort External { get; }
    ushort Internal { get; }
    NatPortMappingStatus Status { get; }
  }

  public interface INatDevice {
    string DeviceType { get; }
    IEnumerable<IPortMapping> Ports { get; }
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

    static public bool Enabled() {
      if (Available()) {
        if (BoltNetworkInternal.NatCommunicator.IsEnabled) {
          return true;
        }

        BoltLog.Error("You must enable UPnP by calling BoltNetwork.EnableUPnP() calling this method");
      }

      return false;
    }

    static public void Update() {
      if (Available() && BoltNetworkInternal.NatCommunicator.IsEnabled) {
        BoltNetworkInternal.NatCommunicator.Update();
      }
    }

    static public bool NextPortStatusChange(out Bolt.INatDevice device, out Bolt.IPortMapping mapping) {
      if (Available() && BoltNetworkInternal.NatCommunicator.IsEnabled) {
        return BoltNetworkInternal.NatCommunicator.NextPortStatusChange(out device, out mapping);
      }

      device = null;
      mapping = null;
      return false;
    }

    static public IEnumerable<Bolt.INatDevice> NatDevices {
      get {
        if (Enabled()) {
          return BoltNetworkInternal.NatCommunicator.NatDevices;
        }
        else {
          return new Bolt.INatDevice[0];
        }
      }
    }

    static public void OpenPort(int port) {
      if (Enabled()) {
        BoltNetworkInternal.NatCommunicator.OpenPort(port);
      }
    }

    static public void ClosePort(int port) {
      if (Enabled()) {
        BoltNetworkInternal.NatCommunicator.ClosePort(port);
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
