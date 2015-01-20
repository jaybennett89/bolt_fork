using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class Context {
    readonly Guid peerId;
    readonly Guid gameId;
    readonly Type[] messageTypes = new Type[256];

    public Guid PeerId {
      get { return peerId; }
    }

    public Guid GameId {
      get { return gameId; }
    }

    public Context(Guid game)
      : this(game, Guid.NewGuid()) {
    }

    public Context(Guid game, Guid peer) {
      gameId = game;
      peerId = peer;

      RegisterMessageType<Protocol.Ack>();
      RegisterMessageType<Protocol.Error>();

      RegisterMessageType<Protocol.BroadcastSearch>();
      RegisterMessageType<Protocol.BroadcastSession>();

      RegisterMessageType<Protocol.PeerConnect>();
      RegisterMessageType<Protocol.PeerConnectResult>();

      RegisterMessageType<Protocol.HostInfo>();
      RegisterMessageType<Protocol.HostRegister>();
      RegisterMessageType<Protocol.HostKeepAlive>();
      RegisterMessageType<Protocol.GetHostList>();

      RegisterMessageType<Protocol.ProbeEndPoint>();
      RegisterMessageType<Protocol.ProbeEndPointResult>();
      RegisterMessageType<Protocol.ProbeHairpin>();
      RegisterMessageType<Protocol.ProbeUnsolicited>();
      RegisterMessageType<Protocol.ProbeFeatures>();

      RegisterMessageType<Protocol.Punch>();
      RegisterMessageType<Protocol.PunchOnce>();
      RegisterMessageType<Protocol.PunchRequest>();

      RegisterMessageType<Protocol.DirectConnection>();
    }

    public T CreateMessage<T>() where T : Protocol.Message {
      for (byte i = 1; i < messageTypes.Length; ++i) {
        if (ReferenceEquals(messageTypes[i], typeof(T))) {
          T msg;

          msg = (T)Activator.CreateInstance(messageTypes[i]);
          msg.Context = this;
          msg.PeerId = peerId;
          msg.GameId = gameId;
          msg.Init(i);

          return msg;
        }
      }

      throw new NotSupportedException();
    }

    public T CreateMessage<T>(Protocol.Query query) where T : Protocol.Result {
      T result;
      
      result = CreateMessage<T>();
      result.Query = query.MessageId;

      return result;
    }

    public Protocol.Message CreateMessage(byte type) {
      if (messageTypes[type] != null) {
        Protocol.Message msg;

        msg = (Protocol.Message)Activator.CreateInstance(messageTypes[type]);
        msg.Context = this;
        msg.Init(type);

        return msg;
      }

      throw new NotSupportedException();
    }

    public int WriteMessage(Message msg, byte[] buffer) {
      UdpLog.Info("Writing: {0}", msg.GetType().Name);
      buffer[0] = Protocol.Message.MESSAGE_HEADER;
      return msg.Serialize(1, buffer, true);
    }

    public Protocol.Message ParseMessage(byte[] bytes) {
      int o = 0;
      return ParseMessage(bytes, ref o);
    }

    public Protocol.Message ParseMessage(byte[] bytes, ref int offset) {
      UdpAssert.Assert(bytes[offset] == Protocol.Message.MESSAGE_HEADER);

      Protocol.Message msg;

      msg = CreateMessage(bytes[offset + 1]);
      msg.Context = this;
      msg.InitBuffer(offset + 1, bytes, false);

      UdpLog.Info("Parsing: {0}", msg.GetType().Name);

      offset = msg.Serialize();
      return msg;
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
  }
}
