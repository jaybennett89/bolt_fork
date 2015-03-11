using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace UdpKit {
  partial class UdpSocket {
    bool PeekInternal(out UdpEvent ev) {
      lock (eventQueueIn) {
        if (eventQueueIn.Count > 0) {
          ev = eventQueueIn.Peek();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    bool PollInternal(out UdpEvent ev) {
      lock (eventQueueIn) {
        if (eventQueueIn.Count > 0) {
          ev = eventQueueIn.Dequeue();
          return true;
        }
      }

      ev = default(UdpEvent);
      return false;
    }

    void ProcessStartEvent() {
      UdpEvent ev;

      while (PollInternal(out ev)) {
        switch (ev.Type) {
          case UdpEvent.INTERNAL_START:
            OnEventStart(ev);
            return;

          case UdpEvent.INTERNAL_STREAM_CREATECHANNEL:
            OnEvent_Stream_CreateChannel(ev);
            break;

          default:
            UdpLog.Error("Can not send event of type {0} before socket has started", ev.Type);
            break;
        }
      }
    }

    void ProcessInternalEvents() {
      UdpEvent ev;

      while (PollInternal(out ev)) {

        switch (ev.Type) {
          case UdpEvent.INTERNAL_CONNECT: OnEventConnect(ev); break;
          case UdpEvent.INTERNAL_CONNECT_CANCEL: OnEventConnectCancel(ev); break;
          case UdpEvent.INTERNAL_ACCEPT: OnEventAccept(ev); break;
          case UdpEvent.INTERNAL_REFUSE: OnEventRefuse(ev); break;
          case UdpEvent.INTERNAL_DISCONNECT: OnEventDisconnect(ev); break;
          case UdpEvent.INTERNAL_CLOSE: OnEventClose(ev); return;
          case UdpEvent.INTERNAL_SEND: OnEventSend(ev); break;

          case UdpEvent.INTERNAL_LANBROADCAST_ENABLE: OnEvent_LanBroadcast_Enable(ev); break;
          case UdpEvent.INTERNAL_LANBROADCAST_DISABLE: OnEvent_LanBroadcast_Disable(ev); break;

          case UdpEvent.INTERNAL_SESSION_HOST_SETINFO: OnEvent_Session_Host_SetInfo(ev); break;
          case UdpEvent.INTERNAL_SESSION_CONNECT: OnEvent_Session_Connect(ev); break;

          case UdpEvent.INTERNAL_MASTERSERVER_CONNECT: OnEvent_MasterServer_Connect(ev); break;
          case UdpEvent.INTERNAL_MASTERSERVER_DISCONNECT: OnEvent_MasterServer_Disconnect(ev); break;
          case UdpEvent.INTERNAL_MASTERSERVER_SESSIONLISTREQUEST: OnEvent_MasterServer_SessionListRequest(ev); break;
          case UdpEvent.INTERNAL_MASTERSERVER_INFOREQUEST: OnEvent_MasterServer_InfoRequest(ev); break;

          case UdpEvent.INTERNAL_STREAM_QUEUE: OnEvent_Stream_Queue(ev); break;
          case UdpEvent.INTERNAL_STREAM_SETBANDWIDTH: OnEvent_Stream_SetBandwidth(ev); break;

          case UdpEvent.INTERNAL_START:
          case UdpEvent.INTERNAL_STREAM_CREATECHANNEL:
            UdpLog.Error("Can not send event of type {0} after the socket has started", ev.Type);
            break;

          default:
            UdpLog.Error("Unknown event type {0}", ev.Type);
            break;
        }
      }
    }


    void OnEventStart(UdpEvent ev) {
      var start = ev.As<UdpEventStart>();

      if (CreatePhysicalSocket(start.EndPoint, UdpSocketState.Running)) {
        // set mode
        mode = start.Mode;

        try {
          // try to find the lan interface ip address
          FindLanInterfaceIP();
        }
        catch (Exception exn) {
          UdpLog.Error(exn.ToString());
        }

        Raise(new UdpEventStartDone { EndPoint = platformSocket.EndPoint, ResetEvent = start.ResetEvent });
      }
      else {
        Raise(new UdpEventStartFailed { ResetEvent = start.ResetEvent });
      }
    }

    void FindLanInterfaceIP() {
      IEnumerable<UdpPlatformInterface> ifaces;

      // ask the platform for all network interfaces
      ifaces = platform.GetNetworkInterfaces();

      foreach (var f in ifaces) {
        UdpLog.Info("{0} {1}", f.UnicastAddresses.Join(", "), f.GatewayAddresses.Join(","));
      }

      // only interfaces which has a unitcast address
      ifaces = ifaces.Where(x => x.UnicastAddresses.Length > 0);

      // only interfaces that has gateways
      ifaces = ifaces.Where(x => x.GatewayAddresses.Length > 0);

      // only interfaces whith gateways addresses that are not 0.0.0.0, 127.0.0.1, 255.255.255.255 or any of our own unitcast addresses
      ifaces = ifaces.Where(x => x.GatewayAddresses.Count(y => y != UdpIPv4Address.Localhost && y != UdpIPv4Address.Any && y != UdpIPv4Address.Broadcast && !x.UnicastAddresses.Contains(y)) > 0);

      if (ifaces.Count() > 0) {
        foreach (var f in ifaces) {
          try {
            UdpIPv4Address ip = f.UnicastAddresses.First(x => x.IsPrivate);
            UdpEndPoint ep = new UdpEndPoint(ip, platformSocket.EndPoint.Port);

            // Set this endpoint
            LANEndPoint = ep;

            // tell session manager
            sessionManager.SetLanEndPoint(LANEndPoint);

            // tell user
            UdpLog.Info("LAN endpoint resolved as {0}", LANEndPoint);

            // we're done
            return;
          }
          catch {

          }
        }
      }

      sessionManager.SetLanEndPoint(UdpEndPoint.Any);
      UdpLog.Info("Could not resolve a possible LAN address by inspecting local network interfaces");
    }

    void OnEventConnect(UdpEvent ev) {
      var connect = ev.As<UdpEventConnectEndPoint>();
      ConnectToEndPoint(connect.EndPoint, connect.Token);
    }

    void ConnectToEndPoint(UdpEndPoint endpoint, byte[] connectToken) {
      if (CheckState(UdpSocketState.Running)) {
        // always stop broadcasting if we join someone
        OnEvent_LanBroadcast_Disable(default(UdpEvent));

        // start joining
        UdpConnection cn = CreateConnection(endpoint, UdpConnectionMode.Client, connectToken);

        if (cn == null) {
          UdpLog.Error("Could not create connection for endpoint {0}", endpoint);
        }
        else {
          UdpLog.Info("Connecting to {0}", endpoint);
        }
      }
    }

    void OnEventConnectCancel(UdpEvent ev) {
      var cancel = ev.As<UdpEventConnectEndPointCancel>();

      if (CheckState(UdpSocketState.Running)) {
        UdpConnection cn;

        if (connectionLookup.TryGetValue(cancel.EndPoint, out cn)) {

          // if we are connecting, destroy connection
          if (cn.CheckState(UdpConnectionState.Connecting)) {

            // tell user this happend
            Raise(new UdpEventConnectFailed { EndPoint = cn.RemoteEndPoint, Token = cn.ConnectToken });

            // destroy this connection
            cn.ChangeState(UdpConnectionState.Destroy);
          }

          // if we are connected, disconnect 
          else if (cn.CheckState(UdpConnectionState.Connected)) {
            cn.SendCommand(UdpConnection.COMMAND_DISCONNECTED);
            cn.ChangeState(UdpConnectionState.Disconnected);
          }
        }
      }
    }

    void OnEventAccept(UdpEvent ev) {
      var accept = ev.As<UdpEventAcceptConnect>();
      var connectToken = default(byte[]);

      if (pendingConnections.TryGetValue(accept.EndPoint, out connectToken)) {

        // remove it
        pendingConnections.Remove(accept.EndPoint);


        AcceptConnection(accept.EndPoint, accept.UserObject, accept.Token, connectToken);
      }
    }

    void OnEventRefuse(UdpEvent ev) {
      var refuse = ev.As<UdpEventRefuseConnect>();

      if (pendingConnections.Remove(refuse.EndPoint)) {
        SendCommand(refuse.EndPoint, UdpConnection.COMMAND_REFUSED, refuse.Token);
      }
    }

    void OnEventDisconnect(UdpEvent ev) {
      var disconnect = ev.As<UdpEventDisconnect>();

      if (disconnect.Connection.CheckState(UdpConnectionState.Connected)) {
        disconnect.Connection.SendCommand(UdpConnection.COMMAND_DISCONNECTED, disconnect.Token);
        disconnect.Connection.ChangeState(UdpConnectionState.Disconnected);
      }
    }

    void OnEventClose(UdpEvent ev) {
      var close = ev.As<UdpEventClose>();

      if (CheckState(UdpSocketState.Running)) {
        foreach (var c in connectionLookup.Values) {
          c.SendCommand(UdpConnection.COMMAND_DISCONNECTED);
          c.ChangeState(UdpConnectionState.Disconnected);
        }
      }

      if (ChangeState(UdpSocketState.Running, UdpSocketState.Shutdown)) {
        // wait 3 seconds for connection stuff to go out
        Thread.Sleep(3000);

        // then close platform socket
        platformSocket.Close();

        if (platformSocket.Error != null) {
          UdpLog.Error("Failed to shutdown socket, platform error: {0}", platformSocket.Error);
        }

        connectionList.Clear();
        connectionLookup.Clear();
        eventQueueIn.Clear();
        pendingConnections.Clear();

        // signal to user thread that this is done
        if (close.ResetEvent != null) {
          close.ResetEvent.Set();
        }
      }
    }

    void OnEventSend(UdpEvent ev) {
      ((UdpConnection)ev.Object0).OnPacketSend((UdpPacket)ev.Object1);
    }

    /*
     * Master Server
     * */

    void OnEvent_MasterServer_Connect(UdpEvent ev) {
      if (masterClient != null) {
        masterClient.Disconnect();
      }

      masterClient = new MasterClient(this, new Protocol.ProtocolClient(platformSocket, GameId, PeerId));
      masterClient.Connect(ev.As<UdpEventMasterServerConnect>().EndPoint);
    }

    void OnEvent_MasterServer_Disconnect(UdpEvent ev) {
      if (masterClient != null) {
        masterClient.Disconnect();
      }
    }

    void OnEvent_MasterServer_SessionListRequest(UdpEvent ev) {
      if (masterClient != null) {
        masterClient.RequestSessionList();
      }
    }

    void OnEvent_MasterServer_InfoRequest(UdpEvent ev) {
      if (masterClient != null) {
        masterClient.RequestZeusInfo();
      }
    }

    /*
     * Session
     * */

    void OnEvent_Session_Host_SetInfo(UdpEvent ev) {
      sessionManager.SetHostInfo(ev.As<UdpEventSessionSetHostData>());
    }

    void OnEvent_Session_Connect(UdpEvent ev) {
      var connect = ev.As<UdpEventSessionConnect>();

      switch (connect.Session.Source) {
        case UdpSessionSource.Zeus:
          if (masterClient != null) {
            masterClient.ConnectToSession(connect.Session, connect.Token);
          }
          else {
            UdpLog.Error("No connection to master server found");
          }
          break;

        case UdpSessionSource.Lan:
          ConnectToEndPoint(connect.Session.LanEndPoint, connect.Token);
          break;

        case UdpSessionSource.Steam:
          UdpLog.Error("Steam session support not implemented");
          break;
      }
    }

    /*
     * LanBroadcast
     * */

    void OnEvent_LanBroadcast_Enable(UdpEvent ev) {
      broadcastManager.Enable(ev.As<UdpEventLanBroadcastEnable>());
    }

    void OnEvent_LanBroadcast_Disable(UdpEvent ev) {
      broadcastManager.Disable();
    }

    /*
     * Stream
     * */

    void OnEvent_Stream_SetBandwidth(UdpEvent ev) {
      var bandwidth = ev.As<UdpEventStreamSetBandwidth>();
      bandwidth.Connection.OnStreamSetBandwidth(bandwidth.BytesPerSecond);
    }

    void OnEvent_Stream_Queue(UdpEvent ev) {
      var queue = ev.As<UdpEventStreamQueue>();

      UdpStreamChannel c;

      if (streamChannels.TryGetValue(queue.StreamOp.Channel, out c) == false) {
        UdpLog.Error("Unknown {0}", queue.StreamOp.Channel);
        return;
      }

      queue.Connection.OnStreamQueue(c, queue.StreamOp);
    }

    void OnEvent_Stream_CreateChannel(UdpEvent ev) {
      UdpStreamChannel c = new UdpStreamChannel();
      c.Config = ev.As<UdpEventStreamCreateChannel>().ChannelConfig;

      if (streamChannels.ContainsKey(c.Name)) {
        UdpLog.Error("Duplicate channel id '{0}', not creating channel '{1}'", c.Name);
        return;
      }

      streamChannels.Add(c.Name, c);
      UdpLog.Debug("Channel {0} created", c.Name);
    }
  }
}
