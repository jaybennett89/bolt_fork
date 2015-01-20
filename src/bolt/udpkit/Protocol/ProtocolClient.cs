using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  class ProtocolClient : Protocol.Context {
    struct AckCallback {
      public UdpEndPoint Filter;
      public Action<Query> Action;
    }

    struct MsgHandler {
      public UdpEndPoint Filter;
      public Action<Message> Action;
    }

    public uint LastSend;
    public byte[] Buffer = new byte[1024];
    public UdpPlatformSocket Socket;

    List<Query> Queries;
    Dictionary<Type, MsgHandler> Handlers;
    Dictionary<Type, AckCallback> Callbacks;

    public UdpPlatform Platform {
      get { return Socket.Platform; }
    }

    public ProtocolClient(UdpPlatformSocket socket, Guid gameId, Guid peerId)
      : base(gameId, peerId) {
      Socket = socket;

      // list of all pending queries
      Queries = new List<Query>();

      // ack handlers
      Callbacks = new Dictionary<Type, AckCallback>();

      // message callbacks
      Handlers = new Dictionary<Type, MsgHandler>();
    }

    public void SetHandler<T>(Action<T> handler) where T : Message {
      SetHandler(handler, new UdpEndPoint(new UdpIPv4Address(255, 255, 255, 255), ushort.MaxValue));
    }

    public void SetHandler<T>(Action<T> handler, UdpEndPoint filter) where T : Message {
      Handlers[typeof(T)] = new MsgHandler {
        Filter = filter,
        Action = m => handler((T)m)
      };
    }

    public void SetCallback<T>(Action<T> callback) where T : Query {
      SetCallback<T>(callback, new UdpEndPoint(new UdpIPv4Address(255, 255, 255, 255), ushort.MaxValue));
    }

    public void SetCallback<T>(Action<T> callback, UdpEndPoint filter) where T : Query {
      Callbacks[typeof(T)] = new AckCallback {
        Filter = filter,
        Action = q => callback((T)q)
      };
    }

    public void Update(uint now) {
      for (int i = 0; i < Queries.Count; ++i) {
        var qry = Queries[i];

        if (qry.Timeout < now) {
          AckCallback callback;

          if (qry.Resend && (qry.Attempts < 10)) {
            // only resend if we have a valid handler
            if (GetValidCallback(qry, out callback)) {
              Send(qry, qry.Target);
            }
            else {
              RemoveQueryAt(ref i);
            }
          }
          else {
            try {
              // if we have a valid handler for this query, invoke it
              if (GetValidCallback(qry, out callback)) {
                callback.Action(qry);
              }
            }
            finally {
              RemoveQueryAt(ref i);
            }
          }
        }
      }
    }

    void RemoveQueryAt(ref int i) {
      // remove it
      Queries.RemoveAt(i);

      // reduce index
      --i;
    }

    bool GetValidCallback(Query qry, out AckCallback callback) {
      if (Callbacks.TryGetValue(qry.GetType(), out callback)) {
        if ((callback.Filter & qry.Target) == qry.Target) {
          return true;
        }
      }

      return false;
    }

    bool GetValidHandler(Message msg, out MsgHandler handler) {
      if (Handlers.TryGetValue(msg.GetType(), out handler)) {
        if ((handler.Filter & msg.Sender) == msg.Sender) {
          return true;
        }
      }

      return false;
    }

    public void Recv(UdpEndPoint endpoint, byte[] buffer, int offset) {
      Message msg;

      msg = ParseMessage(buffer, ref offset);
      msg.Sender = endpoint;

      UdpLog.Info("Received {0} From {1}", msg.GetType().Name, endpoint);

      if (msg is Result) {
        QueryResult(msg as Result);
      }
      else {
        MsgHandler handler;

        if (GetValidHandler(msg, out handler)) {
          handler.Action(msg);
        }
      }
    }

    public void Send(Message msg, UdpEndPoint endpoint) {
      if (msg is Query) {
        Query qry;

        qry = (Query)msg;
        qry.SendTime = Platform.GetPrecisionTime();
        qry.Target = endpoint;
        qry.Attempts = qry.Attempts + 1;
        qry.Timeout = qry.SendTime + (qry.BaseTimeout * qry.Attempts);

        if (qry.Attempts == 1) {
          if (qry.IsUnique) {
            QueryFilter(qry.GetType(), endpoint);
          }

          Queries.Add(qry);
        }
      }

      // log!
      UdpLog.Info("Sending To {0}", endpoint);

      // send mesage
      Socket.SendTo(Buffer, WriteMessage(msg, Buffer), endpoint);

      // 
      LastSend = Platform.GetPrecisionTime();
    }

    void QueryResult(Result result) {
      UdpAssert.Assert(result.Query != Guid.Empty);

      for (int i = 0; i < Queries.Count; ++i) {
        var qry = Queries[i];

        if (qry.MessageId == result.Query) {
          // remove this query
          RemoveQueryAt(ref i);

          // assign result to query
          qry.Result = result;

          AckCallback callback;

          if (GetValidCallback(qry, out callback)) {
            callback.Action(qry);
          }

          return;
        }
      }
    }

    void QueryFilter(Type t, UdpEndPoint endpoint) {
      for (int i = 0; i < Queries.Count; ++i) {
        if ((Queries[i].GetType() == t) && (Queries[i].Target == endpoint)) {
          RemoveQueryAt(ref i);
        }
      }
    }
  }
}
