using System;
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
          case UdpEvent.INTERNAL_FORGETSESSIONS: OnEvent_ForgetSessions(ev); break;

          case UdpEvent.INTERNAL_SESSION_HOST_SETINFO: OnEvent_Session_Host_SetInfo(ev); break;
          case UdpEvent.INTERNAL_SESSION_CONNECT: OnEvent_Session_Connect(ev); break;

          case UdpEvent.INTERNAL_MASTERSERVER_CONNECT: OnEvent_MasterServer_Connect(ev); break;
          case UdpEvent.INTERNAL_MASTERSERVER_DISCONNECT: OnEvent_MasterServer_Disconnect(ev); break;
          case UdpEvent.INTERNAL_MASTERSERVER_SESSION_LISTREQUEST: OnEvent_MasterServer_Session_ListRequest(ev); break;

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

    void OnEvent_MasterServer_Disconnect(UdpEvent ev) {
      if (masterClient != null) {
        masterClient.Disconnect();
      }
    }

    void OnEventStart(UdpEvent ev) {
      if (CreatePhysicalSocket(ev.EndPoint, UdpSocketState.Running)) {
        // set mode
        mode = ev.SocketMode;

        // tell user
        Raise(UdpEvent.PUBLIC_START_DONE, platformSocket.EndPoint);
      }
      else {
        Raise(UdpEvent.PUBLIC_START_FAILED);
      }
    }

    void OnEventConnect(UdpEvent ev) {
      ConnectToEndPoint(ev.EndPoint, ev.ConnectToken);
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
      if (CheckState(UdpSocketState.Running)) {
        UdpConnection cn;

        if (connectionLookup.TryGetValue(ev.EndPoint, out cn)) {
          // if we are connecting, destroy connection
          if (cn.CheckState(UdpConnectionState.Connecting)) {
            // notify user thread
            Raise(UdpEvent.PUBLIC_CONNECT_FAILED, ev.EndPoint);

            // destroy this connection
            cn.ChangeState(UdpConnectionState.Destroy);
          }

          // if we are connected, disconnect 
          else if (ev.Connection.CheckState(UdpConnectionState.Connected)) {
            ev.Connection.SendCommand(UdpConnection.COMMAND_DISCONNECTED);
            ev.Connection.ChangeState(UdpConnectionState.Disconnected);
          }
        }
      }
    }

    void OnEventAccept(UdpEvent ev) {
      if (pendingConnections.Remove(ev.EndPoint)) {
        AcceptConnection(ev.EndPoint, ev.AcceptArgs.UserObject, ev.AcceptArgs.AcceptToken, ev.ConnectToken);
      }
    }

    void OnEventRefuse(UdpEvent ev) {
      if (pendingConnections.Remove(ev.EndPoint)) {
        SendCommand(ev.EndPoint, UdpConnection.COMMAND_REFUSED, ev.RefusedToken);
      }
    }

    void OnEventDisconnect(UdpEvent ev) {
      if (ev.Connection.CheckState(UdpConnectionState.Connected)) {
        ev.Connection.SendCommand(UdpConnection.COMMAND_DISCONNECTED, ev.DisconnectToken);
        ev.Connection.ChangeState(UdpConnectionState.Disconnected);
      }
    }

    void OnEventClose(UdpEvent ev) {
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
        ev.ResetEvent.Set();
      }
    }

    void OnEventSend(UdpEvent ev) {
      ev.Connection.OnPacketSend(ev.Packet);
    }

    /*
     * Master Server
     * */

    void OnEvent_MasterServer_Connect(UdpEvent ev) {
      masterClient = new MasterClient(this, new Protocol.ProtocolClient(platformSocket, GameId, PeerId));
      masterClient.Connect(ev.EndPoint);
    }

    void OnEvent_MasterServer_Session_ListRequest(UdpEvent ev) {
      masterClient.RequestSessionList();
    }

    /*
     * Session
     * */

    void OnEvent_Session_Host_SetInfo(UdpEvent ev) {
      sessionManager.SetHostInfo(ev.HostName, ev.HostData);
    }

    void OnEvent_Session_Connect(UdpEvent ev) {
      switch (ev.Session.Source) {
        case UdpSessionSource.MasterServer:
          if (masterClient != null) {
            masterClient.ConnectToSession(ev.Session);
          }
          else {
            UdpLog.Error("No connection to master server found");
          }
          break;

        case UdpSessionSource.Lan:
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
      broadcastManager.Enable(ev.BroadcastArgs);
    }

    void OnEvent_LanBroadcast_Disable(UdpEvent ev) {
      broadcastManager.Disable();
    }

    void OnEvent_ForgetSessions(UdpEvent ev) {
      sessionManager.ForgetSessions(ev.SessionSource);
    }

    /*
     * Stream
     * */

    void OnEvent_Stream_SetBandwidth(UdpEvent ev) {
      ev.Connection.OnStreamSetBandwidth(ev.ChannelRate);
    }

    void OnEvent_Stream_Queue(UdpEvent ev) {
      UdpStreamChannel c;

      if (streamChannels.TryGetValue(ev.StreamOp.Channel, out c) == false) {
        UdpLog.Error("Unknown {0}", ev.StreamOp.Channel);
        return;
      }

      ev.Connection.OnStreamQueue(c, ev.StreamOp);
    }

    void OnEvent_Stream_CreateChannel(UdpEvent ev) {
      UdpStreamChannel c = new UdpStreamChannel();
      c.Config = ev.ChannelConfig;

      if (streamChannels.ContainsKey(c.Name)) {
        UdpLog.Error("Duplicate channel id '{0}', not creating channel '{1}'", c.Name);
        return;
      }

      streamChannels.Add(c.Name, c);
      UdpLog.Debug("Channel {0} created", c.Name);
    }
  }
}
