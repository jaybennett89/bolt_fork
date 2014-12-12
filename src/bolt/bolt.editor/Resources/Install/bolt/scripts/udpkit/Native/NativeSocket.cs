﻿#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

public class NativeSocket : UdpKit.UdpPlatformSocket {
  NativePlatform platform;

  IntPtr socket;
  bool broadcast;

  public override bool Broadcast {
    get {
      return broadcast;
    }
    set {
      if (value) {
        NativePInvoke.BroadcastEnable(socket);
      }
      else {
        NativePInvoke.BroadcastDisable(socket);
      }

      broadcast = value;
    }
  }

  public override UdpEndPoint EndPoint {
    get {
      UdpEndPoint ep = default(UdpEndPoint);

      if (NativePInvoke.GetEndPoint(socket, out ep) == NativePInvoke.UDPKIT_SOCKET_OK) {
        return ep;
      }

      return UdpEndPoint.Any;
    }
  }

  public override string Error {
    get { return NativePInvoke.Error(); }
  }

  public override bool IsBound {
    get { return EndPoint != UdpEndPoint.Any; }
  }

  public override UdpKit.UdpPlatform Platform {
    get { return platform; }
  }

  public NativeSocket(NativePlatform p) {
    platform = p;
    broadcast = false;
    socket = NativePInvoke.CreateSocket();
  }

  public override void Bind(UdpKit.UdpEndPoint ep) {
    CheckResult(NativePInvoke.Bind(socket, ep));
  }

  public override void Close() {
    CheckResult(NativePInvoke.Close(socket));
    socket = IntPtr.Zero;
  }

  public override int RecvFrom(byte[] buffer, int bufferSize, ref UdpKit.UdpEndPoint endpoint) {
    var sender = default(UdpEndPoint);
    var bytesReceived = NativePInvoke.RecvFrom(socket, buffer, bufferSize, out sender);

    if (bytesReceived > 0) {
      endpoint = sender;
    }

    if (bytesReceived < 0) {
      UdpLog.Error(Error);
      bytesReceived = -1;
    }

    return bytesReceived;
  }

  public override bool RecvPoll(int timeout) {
    return CheckResult(NativePInvoke.RecvPoll(socket, timeout));
  }

  public override bool RecvPoll() {
    return RecvPoll(0);
  }

  public override int SendTo(byte[] buffer, int bytesToSend, UdpKit.UdpEndPoint endpoint) {
    var bytesSent = NativePInvoke.SendTo(socket, buffer, bytesToSend, endpoint);

    if (bytesSent >= 0) {
      return bytesSent;
    }

    UdpLog.Error(Error);
    return -1;
  }

  bool CheckResult(int result) {
    if (result == NativePInvoke.UDPKIT_SOCKET_OK) {
      return true;
    }

    if (result == NativePInvoke.UDPKIT_SOCKET_ERROR) {
      UdpLog.Error(Error);
      return false;
    }

    if (result == NativePInvoke.UDPKIT_SOCKET_NOTVALID) {
      UdpLog.Error("Invalid socket pointer: {0}", socket);
      return false;
    }

    if (result == NativePInvoke.UDPKIT_SOCKET_NODATA) {
      return false;
    }

    throw new Exception(string.Format("Unknown return code: {0}", result));
  }
}
#endif