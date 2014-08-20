using System.Runtime.InteropServices;

namespace UdpKit {
  public enum UdpSendFailReason {
    None,
    StreamOverflow,
    NotConnected,
    PacketWindowFull,
    SocketError,
    SerializerReturnedFalse
  }

  public enum UdpEventType {
    SocketStarted = UdpEvent.PUBLIC_STARTED,
    SocketStartupFailed = UdpEvent.PUBLIC_START_FAILED,
    ConnectRequest = UdpEvent.PUBLIC_CONNECT_REQUEST,
    ConnectFailed = UdpEvent.PUBLIC_CONNECT_FAILED,
    ConnectRefused = UdpEvent.PUBLIC_CONNECT_REFUSED,
    HandshakeFailedOnSize = UdpEvent.PUBLIC_CONNECT_REFUSED_SIZE,
    HandshakeFailedOnValue = UdpEvent.PUBLIC_CONNECT_REFUSED_VALUE,
    Connected = UdpEvent.PUBLIC_CONNECTED,
    Disconnected = UdpEvent.PUBLIC_DISCONNECTED,
    ObjectSendFailed = UdpEvent.PUBLIC_OBJECT_SEND_FAILED,
    ObjectRejected = UdpEvent.PUBLIC_OBJECT_REJECTED,
    ObjectDelivered = UdpEvent.PUBLIC_OBJECT_DELIVERED,
    ObjectLost = UdpEvent.PUBLIC_OBJECT_LOST,
    ObjectReceived = UdpEvent.PUBLIC_OBJECT_RECEIVED,
    ObjectSent = UdpEvent.PUBLIC_OBJECT_SENT,
  }

  [StructLayout(LayoutKind.Explicit)]
  public struct UdpEvent {

    struct UdpEventReferenceObjects {
      public object Object1;
      public object Object0;
    }

    internal const int INTERNAL_START = 1;
    internal const int INTERNAL_CONNECT = 3;
    internal const int INTERNAL_CONNECT_CANCEL = 17;
    internal const int INTERNAL_ACCEPT = 5;
    internal const int INTERNAL_REFUSE = 7;
    internal const int INTERNAL_DISCONNECT = 9;
    internal const int INTERNAL_CLOSE = 11;
    internal const int INTERNAL_SEND = 13;
    internal const int INTERNAL_CONNECTION_OPTION = 15;
    internal const int INTERNAL_SLEEP = 19;
    internal const int INTERNAL_CLOUD_SET_MASTER = 27;
    internal const int INTERNAL_START_CLOUD = 21;
    internal const int INTERNAL_ENABLE_BROADCAST = 23;
    internal const int INTERNAL_DISABLE_BROADCAST = 29;
    internal const int INTERNAL_SET_SESSION_DATA = 25;

    internal const int PUBLIC_CONNECT_REQUEST = 2;
    internal const int PUBLIC_CONNECT_FAILED = 4;

    internal const int PUBLIC_CONNECT_REFUSED = 6;
    internal const int PUBLIC_CONNECT_REFUSED_SIZE = 28;
    internal const int PUBLIC_CONNECT_REFUSED_VALUE = 30;

    internal const int PUBLIC_CONNECTED = 8;
    internal const int PUBLIC_DISCONNECTED = 10;
    internal const int PUBLIC_OBJECT_SEND_FAILED = 12;
    internal const int PUBLIC_OBJECT_REJECTED = 14;
    internal const int PUBLIC_OBJECT_DELIVERED = 16;
    internal const int PUBLIC_OBJECT_LOST = 18;
    internal const int PUBLIC_OBJECT_RECEIVED = 20;
    internal const int PUBLIC_OBJECT_SENT = 22;
    internal const int PUBLIC_STARTED = 24;
    internal const int PUBLIC_START_FAILED = 26;

    [FieldOffset(0)]
    internal int Type;

    [FieldOffset(4)]
    UdpEndPoint endPoint;

    [FieldOffset(4)]
    internal UdpConnectionOption Option;

    [FieldOffset(4)]
    UdpSendFailReason failReason;

    [FieldOffset(8)]
    internal int intVal;

    [FieldOffset(8)]
    internal float floatVal;

    [FieldOffset(16)]
    UdpEventReferenceObjects Refs;

    internal bool IsInternal {
      get { return (Type & 1) == 1; }
    }

    public UdpEventType EventType {
      get { return (UdpEventType) Type; }
    }

    public UdpEndPoint EndPoint {
      get { return endPoint; }
      internal set { endPoint = value; }
    }

    public UdpSendFailReason FailedReason {
      get { return failReason; }
      internal set { failReason = value; }
    }

    public UdpConnection Connection {
      get { return (UdpConnection) Refs.Object1; }
      internal set { Refs.Object1 = value; }
    }

    public object Object0 {
      get { return Refs.Object0; }
      internal set { Refs.Object0 = value; }
    }

    public object Object1 {
      get { return Refs.Object1; }
      internal set { Refs.Object1 = value; }
    }

    public UdpStream Stream {
      get { return Object0 as UdpStream; }
    }
  }
}
