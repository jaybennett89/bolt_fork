using System;
using System.Text;
using System.Collections.Generic;

namespace UdpKit {
  public abstract class UdpPlatform {
    readonly Guid id = Guid.NewGuid();
    readonly Type[] messageTypes = new Type[128];

    protected UdpPlatform() {
      RegisterMessageType<Protocol.Ack>();

      RegisterMessageType<Protocol.Socket_Ping>();
      RegisterMessageType<Protocol.Socket_Punch>();

      RegisterMessageType<Protocol.MasterServer_HostKeepAlive>();
      RegisterMessageType<Protocol.MasterServer_HostRegister>();

      RegisterMessageType<Protocol.MasterServer_Session_Info>();
      RegisterMessageType<Protocol.MasterServer_Session_ListRequest>();

      RegisterMessageType<Protocol.MasterServer_NatProbeInfo>();
      RegisterMessageType<Protocol.MasterServer_NatProbeInfo_Result>();

      RegisterMessageType<Protocol.MasterServer_IntroduceInfo>();
      RegisterMessageType<Protocol.MasterServer_Introduce>();
      RegisterMessageType<Protocol.MasterServer_Introduce_Result>();

      RegisterMessageType<Protocol.NatProbe_TestEndPoint>();
      RegisterMessageType<Protocol.NatProbe_TestEndPoint_Result>();
      RegisterMessageType<Protocol.NatProbe_TestHairpin>();
      RegisterMessageType<Protocol.NatProbe_TestUnsolicited>();

      RegisterMessageType<Protocol.NatPunch_PeerRegister>();
      RegisterMessageType<Protocol.NatPunch_PunchInfo>();
    }

    void RegisterMessageType<T>() where T : Protocol.Message {
      for (byte i = 1; i < messageTypes.Length; ++i) {
        if (messageTypes[i] == null) {
          messageTypes[i] = typeof(T);
          return;
        }
      }

      throw new IndexOutOfRangeException();
    }

    internal T CreateMessage<T>() where T : Protocol.Message {
      for (byte i = 1; i < messageTypes.Length; ++i) {
        if (ReferenceEquals(messageTypes[i], typeof(T))) {
          T msg;

          msg = (T)Activator.CreateInstance(messageTypes[i]);
          msg.PeerId = Id;
          msg.Init(i);

          return msg;
        }
      }

      throw new NotSupportedException();
    }

    internal Protocol.Message CreateMessage(byte type) {
      if (messageTypes[type] != null) {
        Protocol.Message msg;

        msg = (Protocol.Message)Activator.CreateInstance(messageTypes[type]);
        msg.Init(type);

        return msg;
      }

      throw new NotSupportedException();
    }

    internal Protocol.Message ParseMessage(byte[] bytes, ref int offset) {
      UdpAssert.Assert(bytes[offset] == Protocol.Message.MESSAGE_HEADER);

      Protocol.Message msg;

      msg = CreateMessage(bytes[offset + 1]);
      msg.InitBuffer(offset + 1, bytes, false);

      offset = msg.Serialize();

      UdpLog.Debug("IN: {0}", msg.GetType());
      return msg;
    }

    public Guid Id { get { return id; } }
    public object Token { get; set; }

    public abstract bool SupportsBroadcast { get; }
    public abstract uint GetPrecisionTime();

    public abstract UdpIPv4Address GetBroadcastAddress();
    public abstract UdpPlatformSocket CreateSocket();
    public abstract List<UdpPlatformInterface> GetNetworkInterfaces();

    public UdpPlatformSocket CreateSocket(UdpEndPoint endpoint) {
      UdpPlatformSocket socket;

      socket = CreateSocket();
      socket.Bind(endpoint);

      return socket;
    }
  }
}
