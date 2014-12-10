using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class Peer {
    public uint LastRecv;
    public uint LastSend;
    public UdpPlatformSocket Socket;

    List<Query> Query_List;
    Dictionary<Type, Action<Query>> Ack_Handlers;
    Dictionary<Type, Action<Message>> Message_Handlers;

    public UdpPlatform Platform {
      get { return Socket.Platform; }
    }

    public Peer() {
      // list of all pending queries
      Query_List = new List<Query>();

      // ack handlers
      Ack_Handlers = new Dictionary<Type, Action<Query>>();

      // message handlers
      Message_Handlers = new Dictionary<Type, Action<Message>>();
    }

    public Peer(UdpPlatformSocket socket)
      : this() {
      Socket = socket;
    }

    public T Message_Create<T>() where T : Message {
      return Platform.CreateMessage<T>();
    }

    public bool HasHandler(Message msg) {
      if (msg is Result) {
        var result = (Result)msg;

        for (int i = 0; i < Query_List.Count; ++i) {
          if (Query_List[i].MessageId == result.Query) {
            return true;
          }
        }

        return false;
      }
      else {
        return Message_Handlers.ContainsKey(msg.GetType());
      }
    }

    public void Message_Create<T>(out T msg) where T : Message {
      msg = Platform.CreateMessage<T>();
    }

    public void Message_AddHandler<T>(Action<T> handler) where T : Message {
      Message_Handlers.Add(typeof(T), q => handler((T)q));
    }

    public void Ack_AddHandler<T>(Action<T> handler) where T : Query {
      Ack_Handlers.Add(typeof(T), q => handler((T)q));
    }

    public void Query_Update(uint now) {
      for (int i = 0; i < Query_List.Count; ++i) {
        if (Query_List[i].Timeout < now) {
          if (Query_List[i].Resend && (Query_List[i].Attempts < 10)) {
            // resend this request
            Message_Send(Query_List[i], Query_List[i].Target);

          }
          else {
            // failed
            var qry = Query_List[i];
            var type = qry.GetType();

            // call handler
            Ack_Handlers[type](qry);

            // remove it
            Query_List.RemoveAt(i);

            // reduce index
            --i;
          }
        }
      }
    }

    public void Message_Recv(int timeout) {
      if (Socket.RecvPoll(timeout)) {
        Message_Recv(Socket.RecvProtocolMessage());
      }
    }

    public void Message_Recv(byte[] buffer, ref int offset) {
      Message_Recv(Platform.ParseMessage(buffer, ref offset));
    }

    public void Message_Recv(Message msg) {
      if (msg is Result) {
        Query_Ack(msg as Result);
      }
      else {
        Message_Handlers[msg.GetType()](msg);
      }
    }

    public void Message_Send<T>(UdpEndPoint endpoint) where T : Message {
      Message_Send(Message_Create<T>(), endpoint);
    }

    public void Message_Ack(Query<Ack> msg) {
      Ack ack;

      ack = Message_Create<Ack>();
      ack.Query = msg.MessageId;

      Message_Send(ack, msg.Sender);
    }

    public void Message_Send(Message msg, UdpEndPoint endpoint) {
      if (msg is Query) {
        Query qry;

        qry = (Query)msg;
        qry.SendTime = Platform.GetPrecisionTime();
        qry.Target = endpoint;
        qry.Attempts = qry.Attempts + 1;
        qry.Timeout = qry.SendTime + (qry.BaseTimeout * qry.Attempts);

        if (qry.Attempts == 1) {
          if (qry.IsUnique) {
            Query_FilterUnique(qry.GetType(), endpoint);
          }

          Query_List.Add(qry);
        }
      }

      // send mesage
      Socket.SendProtocolMessage(endpoint, msg);

      // 
      LastSend = Platform.GetPrecisionTime();
    }

    void Query_Ack(Result result) {
      UdpAssert.Assert(result.Query != Guid.Empty);

      for (int i = 0; i < Query_List.Count; ++i) {
        var qry = Query_List[i];

        if (qry.MessageId == result.Query) {
          Query_List.RemoveAt(i);

          // assign result to query
          qry.Result = result;

          // call ack handler
          if (Ack_Handlers.ContainsKey(qry.GetType())) {
            Ack_Handlers[qry.GetType()](qry);
          }

          return;
        }
      }
    }

    void Query_FilterUnique(Type t, UdpEndPoint endpoint) {
      for (int i = 0; i < Query_List.Count; ++i) {
        if ((Query_List[i].GetType() == t) && (Query_List[i].Target == endpoint)) {

          // remove this one
          Query_List.RemoveAt(i);

          // step index back
          --i;
        }
      }
    }
  }
}
