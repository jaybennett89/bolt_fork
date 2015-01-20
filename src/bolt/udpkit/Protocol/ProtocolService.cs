using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  class ProtocolService {
    ProtocolClient client;

    public ProtocolClient Client {
      get { return client; }
    }

    public uint SendTime {
      get;
      private set;
    }

    public ProtocolService(ProtocolClient p) {
      client = p;
    }

    public void Send<T>(UdpEndPoint endpoint) where T : Protocol.Message {
      Send(endpoint, client.CreateMessage<T>());
    }

    public void Send(UdpEndPoint endpoint, Protocol.Message msg) {
      // update send time
      SendTime = client.Platform.GetPrecisionTime();

      // on your way
      client.Send(msg, endpoint);
    }

    public void Send<T>(UdpEndPoint endpoint, Action<T> setup) where T : Message {
      T msg = client.CreateMessage<T>();

      setup(msg);

      // update send time
      SendTime = client.Platform.GetPrecisionTime();

      // on your way
      client.Send(msg, endpoint);
    }
  }
}
