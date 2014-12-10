using System.Runtime.InteropServices;

namespace UdpKit {
  public enum UdpSendFailReason {
    None,
    NotConnected,
    PacketWindowFull
  }

  public enum UdpEventType {
    SocketStartupDone = UdpEvent.PUBLIC_START_DONE,
    SocketStartupFailed = UdpEvent.PUBLIC_START_FAILED,

    ConnectRequest = UdpEvent.PUBLIC_CONNECT_REQUEST,
    ConnectFailed = UdpEvent.PUBLIC_CONNECT_FAILED,
    ConnectRefused = UdpEvent.PUBLIC_CONNECT_REFUSED,
    ConnectAttempt = UdpEvent.PUBLIC_CONNECT_ATTEMPT,

    Connected = UdpEvent.PUBLIC_CONNECTED,
    Disconnected = UdpEvent.PUBLIC_DISCONNECTED,

    PacketLost = UdpEvent.PUBLIC_PACKET_LOST,
    PacketReceived = UdpEvent.PUBLIC_PACKET_RECEIVED,
    PacketDelivered = UdpEvent.PUBLIC_PACKET_DELIVERED,

    StreamDataReceived = UdpEvent.PUBLIC_STREAM_DATARECEIVED,
    SessionListUpdated = UdpEvent.PUBLIC_SESSION_LISTUPDATED,
  }

  class UdpEventAcceptArgs {
    public object UserObject;
    public byte[] AcceptToken;
    public byte[] ConnectToken;
  }

  [StructLayout(LayoutKind.Explicit)]
  public struct UdpEvent {
    internal const int INTERNAL_START = 1;
    internal const int INTERNAL_CONNECT = 3;
    internal const int INTERNAL_CONNECT_CANCEL = 17;
    internal const int INTERNAL_ACCEPT = 5;
    internal const int INTERNAL_REFUSE = 7;
    internal const int INTERNAL_DISCONNECT = 9;
    internal const int INTERNAL_CLOSE = 11;
    internal const int INTERNAL_SEND = 13;

    internal const int INTERNAL_LANBROADCAST_ENABLE = 23;
    internal const int INTERNAL_LANBROADCAST_DISABLE = 29;

    internal const int INTERNAL_LANBROADCAST_FORGETSESSIONS = 31;

    internal const int INTERNAL_STREAM_CREATECHANNEL = 35;
    internal const int INTERNAL_STREAM_QUEUE = 37;
    internal const int INTERNAL_STREAM_SETBANDWIDTH = 39;


    internal const int INTERNAL_SESSION_CONNECT = 45;
    internal const int INTERNAL_SESSION_HOST_SETINFO = 25;

    internal const int INTERNAL_MASTERSERVER_SET = 41;
    internal const int INTERNAL_MASTERSERVER_SESSION_LISTREQUEST = 43;

    internal const int PUBLIC_SESSION_LISTUPDATED = 36;

    internal const int PUBLIC_CONNECT_REQUEST = 2;
    internal const int PUBLIC_CONNECT_FAILED = 4;
    internal const int PUBLIC_CONNECT_REFUSED = 6;

    internal const int PUBLIC_CONNECTED = 8;
    internal const int PUBLIC_CONNECT_ATTEMPT = 32;
    internal const int PUBLIC_DISCONNECTED = 10;

    internal const int PUBLIC_PACKET_LOST = 18;
    internal const int PUBLIC_PACKET_RECEIVED = 20;
    internal const int PUBLIC_PACKET_DELIVERED = 16;

    internal const int PUBLIC_STREAM_DATARECEIVED = 34;

    internal const int PUBLIC_START_DONE = 24;
    internal const int PUBLIC_START_FAILED = 26;

    [FieldOffset(0)]
    internal int Type;

    [FieldOffset(4)]
    UdpEndPoint endpoint;

    [FieldOffset(4)]
    int integer0;

    [FieldOffset(8)]
    int integer1;

    [FieldOffset(12)]
    int integer2;

    [FieldOffset(16)]
    object Object0;

    [FieldOffset(24)]
    object Object1;

    internal int ChannelRate {
      get { return integer0; }
      set { integer0 = value; }
    }

    internal int ChannelNameId {
      get { return integer0; }
      set { integer0 = value; }
    }

    internal int ChannelPriority {
      get { return integer1; }
      set { integer1 = value; }
    }

    internal UdpChannelMode ChannelMode {
      get { return (UdpChannelMode)integer2; }
      set { integer2 = (int)value; }
    }

    internal string ChannelNameString {
      get { return (string)Object0; }
      set {
        UdpAssert.Assert(Object0 == null);
        Object0 = value;
      }
    }

    //internal bool IsServer {
    //  get { return integer2 == 1; }
    //  set { integer2 = value ? 1 : 0; }
    //}

    internal UdpSocketMode SocketMode {
      get { return (UdpSocketMode)integer2; }
      set { integer2 = (int)value; }
    }

    internal UdpChannelName ChannelName {
      get { return new UdpChannelName(ChannelNameId, (string)Object0); }
      set {
        ChannelNameId = value.Id;
        ChannelNameString = value.Name;
      }
    }

    internal UdpChannelConfig ChannelConfig {
      get {
        UdpChannelConfig channel;
        channel.ChannelName.Name = ChannelNameString;
        channel.ChannelName.Id = ChannelNameId;
        channel.Priority = ChannelPriority;
        channel.Mode = ChannelMode;
        return channel;
      }
      set {
        ChannelNameString = value.ChannelName.Name;
        ChannelNameId = value.ChannelName.Id;
        ChannelPriority = value.Priority;
        ChannelMode = value.Mode;
      }
    }

    public UdpEventType EventType {
      get { return (UdpEventType)Type; }
    }

    public UdpEndPoint EndPoint {
      get { return endpoint; }
      internal set { endpoint = value; }
    }

    public UdpSendFailReason FailedReason {
      get { return (UdpSendFailReason)integer0; }
      internal set { integer0 = (int)value; }
    }

    /*
     * OBJECT 1
     * */

    public UdpConnection Connection {
      get { return (UdpConnection)Object1; }
      set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    }

    public byte[] HostData {
      get { return (byte[])Object1; }
      set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    }

    public byte[] ConnectToken {
      get { return (byte[])Object1; }
      set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    }

    public Map<System.Guid, UdpSession> SessionList {
      get { return (Map<System.Guid, UdpSession>)Object1; }
      set { UdpAssert.Assert(Object1 == null); Object1 = value; }
    }

    /* 
     * OBJECT 0
     * */

    public UdpSession Session {
      get { return (UdpSession)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    public UdpPacket Packet {
      get { return (UdpPacket)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    public byte[] RefusedToken {
      get { return (byte[])Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    public UdpStreamData StreamData {
      get { return (UdpStreamData)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    public byte[] DisconnectToken {
      get { return (byte[])Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    public string HostName {
      get { return (string)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    internal UdpEventAcceptArgs AcceptArgs {
      get { return (UdpEventAcceptArgs)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    internal UdpStreamOp StreamOp {
      get { return (UdpStreamOp)Object0; }
      set { UdpAssert.Assert(Object0 == null); Object0 = value; }
    }

    internal bool IsInternal {
      get { return (Type & 1) == 1; }
    }


  }
}
