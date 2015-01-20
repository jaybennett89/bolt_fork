using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  public class Master : MasterThread {
    readonly NatProber Prober;
    readonly NatPuncher Puncher;
    readonly HostLookup Hosts;
    readonly Protocol.Service Service;

    public Master(UdpPlatform platform)
      : base(platform) {
      Service = new Protocol.Service();
      Service.Peer.Socket = platform.CreateSocket();

      Hosts = new HostLookup(platform);
      Prober = new NatProber(platform);
      Puncher = new NatPuncher(platform);
    }

    protected override void OnInit() {
      // setup message handlers
      Service.Peer.SetHandler<Proto.HostRegister>(OnHostRegister);
      Service.Peer.SetHandler<Proto.HostKeepAlive>(OnHostKeepAlive);
      Service.Peer.SetHandler<Proto.GetHostList>(OnGetHostList);

      // bind to our master server endpoint
      Service.Peer.Socket.Bind(Config.Master);

      // start nat prober
      Prober.Start(Config);

      // start nat puncher
      Puncher.Start(Config);
    }

    protected override void OnUpdate() {
      uint now = Platform.GetPrecisionTime();

      // update message timeouts
      Service.Peer.Update(now);

      // check host timesouts
      Hosts.Timeout(now);

      // receive messages on socket
      Service.Peer.Recv(1);
    }

    void OnHostRegister(Protocol.HostRegister msg) {
      msg.Host._id = msg.PeerId;
      msg.Host._wanEndPoint = msg.Sender;
      msg.Host._lastSeen = Platform.GetPrecisionTime();

      Hosts.Update(msg.Host);

      UdpLog.Info("Registered Host {0}", msg.Host.WanEndPoint);
    }

    void OnHostKeepAlive(Protocol.HostKeepAlive msg) {
      Hosts.KeepAlive(msg.PeerId);
    }

    void OnGetHostList(Protocol.GetHostList msg) {
      foreach (var host in Hosts.All) {
        Service.Send<Proto.HostInfo>(info => info.Host = host, msg.Sender);
      }
    }
  }
}
