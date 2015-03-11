using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpConnection {
    internal const byte COMMAND_CONNECT = 1;
    internal const byte COMMAND_ACCEPTED = 2;
    internal const byte COMMAND_REFUSED = 3;
    internal const byte COMMAND_DISCONNECTED = 4;
    internal const byte COMMAND_PING = 5;

    internal void OnCommandReceived(byte[] buffer, int size) {
      RecvTime = Socket.GetCurrentTime();

      switch (buffer[1]) {
        case COMMAND_CONNECT:
          OnCommandConnect(buffer);
          break;

        case COMMAND_ACCEPTED:
          OnCommandAccepted(buffer, size);
          break;

        case COMMAND_REFUSED:
          OnCommandRefused(buffer, size);
          break;

        case COMMAND_DISCONNECTED:
          OnCommandDisconnected(buffer, size);
          break;

        case COMMAND_PING:
          OnCommandPing(buffer);
          break;

        default:
          ConnectionError(UdpConnectionError.IncorrectCommand);
          break;
      }
    }

    internal void SendCommand(byte cmd) {
      SendCommand(cmd, null);
    }

    internal void SendCommand(byte cmd, byte[] data) {
      UpdateSendTime();
      Socket.SendCommand(EndPoint, cmd, data);
    }

    bool SendCommandConnect() {
      if (ConnectAttempts < Socket.Config.ConnectRequestAttempts) {
        if (ConnectAttempts != 0) {
          UdpLog.Info("{0} retrying connect ...", EndPoint.ToString());
        }

        // notify user
        Socket.Raise(new UdpEventConnectAttempt { EndPoint = RemoteEndPoint, Token = ConnectToken });

        // send connect command on the wire
        SendCommand(COMMAND_CONNECT, ConnectToken);

        ConnectTimeout = Socket.GetCurrentTime() + Socket.Config.ConnectRequestTimeout;
        ConnectAttempts += 1u;
        return true;
      }

      return false;
    }

    void OnCommandConnect(byte[] buffer) {
      if (IsServer) {
        if (CheckState(UdpConnectionState.Connected)) {
          SendCommand(COMMAND_ACCEPTED, AcceptTokenWithPrefix);
        }
      }
      else {
        ConnectionError(UdpConnectionError.IncorrectCommand);
      }
    }

    void OnCommandAccepted(byte[] buffer, int size) {
      if (IsClient) {
        UdpLog.Info("Connect to {0} accepted", RemoteEndPoint);

        if (CheckState(UdpConnectionState.Connecting)) {
          if (size > 6) {
            AcceptToken = new byte[size - 6];
            Buffer.BlockCopy(buffer, 6, AcceptToken, 0, size - 6);
          }

          AcceptTokenWithPrefix = new byte[size - 2];
          Buffer.BlockCopy(buffer, 2, AcceptTokenWithPrefix, 0, size - 2);

          // grab connection id from token with prefix
          ConnectionId = BitConverter.ToUInt32(AcceptTokenWithPrefix, 0);

          if (ConnectionId < 2) {
            UdpLog.Error("Incorrect connection id #{0} received from server", ConnectionId);
          }
          else {
            UdpLog.Info("Correct connection id #{0} received from server", ConnectionId);
          }

          // gotta be larger than 1
          UdpAssert.Assert(ConnectionId > 1u);

          // done!
          ChangeState(UdpConnectionState.Connected);
        }
      }
      else {
        ConnectionError(UdpConnectionError.IncorrectCommand);
      }
    }

    void OnCommandRefused(byte[] buffer, int size) {
      if (IsClient) {
        if (CheckState(UdpConnectionState.Connecting)) {
          // tell user
          Socket.Raise(new UdpEventConnectRefused { EndPoint = this.RemoteEndPoint, Token = UdpUtils.ReadToken(buffer, size, 2) });

          // destroy this connection on next timeout check
          ChangeState(UdpConnectionState.Destroy);
        }
      }
      else {
        ConnectionError(UdpConnectionError.IncorrectCommand);
      }
    }

    void OnCommandDisconnected(byte[] buffer, int size) {
      if (CheckState(UdpConnectionState.Connected)) {
        ChangeState(UdpConnectionState.Disconnected, UdpUtils.ReadToken(buffer, size, 2));
      }
    }

    void OnCommandPing(byte[] buffer) {
    }
  }
}
