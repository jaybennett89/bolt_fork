using System;
using System.Collections.Generic;
using System.Threading;

#if WINDOWS_PHONE || NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Microsoft.Phone.Net.NetworkInformation;
#endif

namespace UdpKit {
  public class Wp8Socket : Wp8SocketBase {
    bool bound;

#if WINDOWS_PHONE || NETFX_CORE
    readonly Action<string> log;
    readonly DatagramSocket dgram;
    readonly AutoResetEvent sendEvent;

    readonly Stack<byte[]> pool = new Stack<byte[]>();

    readonly Queue<Wp8Packet> packets = new Queue<Wp8Packet>();
#endif

    public Wp8Socket(Action<string> logging, int bufferSize)
      : base(bufferSize) {

#if WINDOWS_PHONE || NETFX_CORE
      // logger
      log = logging;

      // socket
      dgram = new DatagramSocket();

      sendEvent = new AutoResetEvent(true);
      sendEvent.Set();
#endif
    }

    public object LocalAddress {
#if WINDOWS_PHONE || NETFX_CORE
      get { return dgram.Information.LocalAddress; }
#else
      get { return null; }
#endif
    }

    public object LocalPort {
#if WINDOWS_PHONE || NETFX_CORE
      get { return dgram.Information.LocalPort; }
#else
      get { return null; }
#endif
    }

    public void Bind(string port) {
#if WINDOWS_PHONE || NETFX_CORE
      lock (syncroot) {
        // bind socket
        dgram.MessageReceived += dgram_MessageReceived;
        dgram.BindServiceNameAsync(port).AsTask().Wait();

        // we're bound
        bound = true;
      }
#endif
    }

#if WINDOWS_PHONE || NETFX_CORE
    void dgram_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args) {
      lock (syncroot) {
        var reader = args.GetDataReader();

        var packet = new Wp8Packet();
        packet.Size = (int)reader.UnconsumedBufferLength;
        packet.Buffer = NewBuffer();
        packet.Host = args.RemoteAddress.RawName;
        packet.Port = args.RemotePort;

        for (int i = 0; i < packet.Size; ++i) {
          packet.Buffer[i] = reader.ReadByte();
        }

        packets.Enqueue(packet);

        //log(string.Format("RECV: {0}:{1} ({2} bytes)", args.RemoteAddress, args.RemotePort, packet.Size));
      }
    }
#endif

    public void Close() {
#if WINDOWS_PHONE || NETFX_CORE
      lock (syncroot) {
        dgram.Dispose();
        bound = false;
      }
#endif
    }

    public bool IsBound {
      get { return bound; }
    }

    public bool RecvPoll(int timeout) {
#if WINDOWS_PHONE || NETFX_CORE
      lock (syncroot) {
        if (packets.Count > 0) {
          return true;
        }
      }

#if WINDOWS_PHONE
      if (timeout > 0) {
        Thread.Sleep(timeout);
      }
#endif

      lock (syncroot) {
        if (packets.Count > 0) {
          return true;
        }
      }
#endif

      return false;
    }

    public int RecvFrom(byte[] buffer, int bufferSize, ref string host, ref string port) {
#if WINDOWS_PHONE || NETFX_CORE
      lock (syncroot) {
        if (packets.Count > 0) {
          // grab packet
          var packet = packets.Dequeue();

          host = packet.Host;
          port = packet.Port;

          // copy data from packet
          System.Buffer.BlockCopy(packet.Buffer, 0, buffer, 0, packet.Size);

          // return buffer to pool
          FreeBuffer(packet.Buffer);

          // return size
          return packet.Size;
        }
      }
#endif

      return 0;
    }

    public int SendTo(byte[] buffer, int bytesToSend, string host, string port) {
#if WINDOWS_PHONE || NETFX_CORE
      if (sendEvent.WaitOne()) {
        SendToAsync(buffer, bytesToSend, host, port);
      }
#endif

      return bytesToSend;
    }

#if WINDOWS_PHONE || NETFX_CORE
    async void SendToAsync(byte[] buffer, int bytesToSend, string host, string port) {
      var stream = await dgram.GetOutputStreamAsync(new HostName(host), port);
      var writer = new DataWriter(stream);

      for (int i = 0; i < bytesToSend; ++i) {
        writer.WriteByte(buffer[i]);
      }

      await writer.StoreAsync();

      writer.DetachStream();
      stream.Dispose();
      writer.Dispose();

      // signal that we can continue
      sendEvent.Set();

      // meep
      // log(string.Format("Sent: {0}:{1}", host, port));
    }
#endif
  }
}
