using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  struct UdpPipeConfig {
    public const int TYPE_BYTES = 1;
    public const int PING_BYTES = 2;

    public byte PipeId;
    public bool UpdatePing;
    public uint Timeout;

    public int WindowSize;
    public int DatagramSize;

    public int AckBytes;
    public int SequenceBytes;

    public int SequenceBits {
      get { return SequenceBytes * 8; }
    }

    public int AckBits {
      get { return AckBytes * 8; }
    }

    public int HeaderSize {
      get { return TYPE_BYTES + PING_BYTES + SequenceBytes + SequenceBytes + AckBytes; }
    }

    public int HeaderSizeBits {
      get { return HeaderSize * 8; }
    }

    public uint NextSequence(uint seq) {
      seq += 1u;
      seq &= ((1u << (SequenceBytes * 8)) - 1);
      return seq;
    }

    public int Distance(uint from, uint to) {
      int shift = (4 - SequenceBytes) * 8;

      to <<= shift;
      from <<= shift;

      return ((int)(from - to)) >> shift;
    }
  }

  class UdpPipe {
    public const int PIPE_COMMAND = 1;
    public const int PIPE_PACKET = 3;
    public const int PIPE_STREAM = 4;
    public const int PIPE_STREAM_UNRELIABLE = 5;
    public const int PIPE_MASTERSERVER = 2;

    struct SendInfo {
      public uint Sequence;
      public UdpRingBuffer<UdpPipeHandle> Window;
    }

    struct RecvInfo {
      public int PendingAcks;
      public UdpRingBuffer<Ack> History;
    }

    struct Ack {
      public readonly uint Time;
      public readonly bool Received;
      public readonly uint Sequence;

      Ack(uint sequence, bool received, uint time) {
        Time = time;
        Sequence = sequence;
        Received = received;
      }

      internal static Ack Lost(uint sequence) {
        return new Ack(sequence, false, 0);
      }

      internal static Ack Recv(uint sequence, uint time) {
        return new Ack(sequence, true, time);
      }
    }

    SendInfo Send;
    RecvInfo Recv;

    public UdpPipeConfig Config;
    public UdpConnection Connection;

    public UdpSocket Socket {
      get { return Connection.Socket; }
    }

    public byte Id {
      get { return Config.PipeId; }
    }

    public float FillRatio {
      get { return Send.Window.FillRatio; }
    }

    public uint LastSend {
      get { return Send.Window.LastOrDefault.Time; }
    }

    public UdpPipe(UdpConnection connection, UdpPipeConfig config) {
      Config = config;
      Connection = connection;

      Send.Window = new UdpRingBuffer<UdpPipeHandle>((int)Config.WindowSize);
      Send.Window.AutoFree = false;

      Recv.History = new UdpRingBuffer<Ack>((int)Config.WindowSize);
      Recv.History.AutoFree = true;

      UdpAssert.Assert(UdpMath.IsMultipleOf8(Config.HeaderSizeBits));
    }

    public void CheckTimeouts(uint now) {
      if (Config.Timeout > 0) {
        var timeout = ((uint)(Connection.NetworkPing * 1250u)) + Config.Timeout;
        var timeoutHalf = ((uint)(Connection.NetworkPing * 1250u)) + (Config.Timeout / 2);

        while (Send.Window.Empty == false) {
          if ((Send.Window.First.Time + timeout) < now) {
            Lost(Send.Window.Dequeue());
          }
          else {
            break;
          }
        }

        if ((Recv.PendingAcks > 0) && ((Send.Window.FirstOrDefault.Time + timeoutHalf) < now)) {
          SendAckPacket();
        }
      }
      else {
        if (Recv.PendingAcks > (Config.WindowSize / 2)) {
          SendAckPacket();
        }
      }
    }

    void SendAckPacket() {
      var buffer = Socket.GetSendBuffer();

      if (WriteHeader(buffer, null)) {
        Socket.Send(Connection.RemoteEndPoint, buffer, Config.HeaderSize);
      }
    }

    public void Disconnected() {
      while (Send.Window.Empty == false) {
        Lost(Send.Window.Dequeue());
      }
    }

    public bool WriteHeader(byte[] buffer, object obj) {
      int o = 0;

      if (Send.Window.Full) {
        return false;
      }

      UdpPipeHandle h;

      h.Time = Socket.GetCurrentTime();
      h.Sequence = Send.Sequence = Config.NextSequence(Send.Sequence);
      h.Object = obj;

      Blit.PackByte /**/ (buffer, ref o, Config.PipeId); // pipe id
      Blit.PackU32  /**/ (buffer, ref o, h.Sequence, Config.SequenceBytes); // packet sequence

      if (Recv.History.Count > 0) {
        UdpAssert.Assert(Recv.History.Last.Received == true);

        uint ackTime = UdpMath.Clamp(Socket.GetCurrentTime() - Recv.History.Last.Time, 0, (1 << 16) - 1);

        Blit.PackU32(buffer, ref o, ackTime, 2); // ack time
        Blit.PackU32(buffer, ref o, Recv.History.Last.Sequence, Config.SequenceBytes); // ack mask
      }
      else {
        UdpAssert.Assert(Recv.History.LastOrDefault.Received == false);

        Blit.PackU32(buffer, ref o, 0, 2); // ack time
        Blit.PackU32(buffer, ref o, 0, Config.SequenceBytes); // ack mask
      }

      for (int i = 0; i < Config.AckBits; ++i) {
        if (i < Recv.History.Count) {
          int ack = (Recv.History.Count - 1) - i;

          if (Recv.History[ack].Received) {
            int bo = o + (i >> 3);
            var bit = (byte)(1 << (i % 8));

            // this bit can't already have been set
            UdpAssert.Assert((buffer[bo] & bit) == 0);

            // set flag
            buffer[bo] |= bit;
          }
        }
      }

      // put handle in window
      Send.Window.Enqueue(h);

      // 
      Connection.UpdateSendTime();

      // mark pending ack as zero
      Recv.PendingAcks = 0;

      // make sure sizes match
      UdpAssert.Assert(((o * 8) + Config.AckBits) == Config.HeaderSizeBits);

      return true;
    }

    public bool ReadHeader(byte[] buffer, int size) {
      UdpAssert.Assert(size >= Config.HeaderSize);
      UdpAssert.Assert(buffer[0] == Config.PipeId);

      int o = 1;

      var packetSequence = Blit.ReadU32(buffer, ref o, Config.SequenceBytes);
      var packetSequenceDistance = Config.Distance(packetSequence, Recv.History.LastOrDefault.Sequence);

      if ((packetSequenceDistance > Config.WindowSize) || (packetSequenceDistance < -Config.WindowSize)) {
        Connection.ConnectionError(UdpConnectionError.SequenceOutOfBounds);
        return false;
      }

      if (packetSequenceDistance <= 0) {
        return false;
      }

      for (int i = 1; i < packetSequenceDistance; ++i) {
        uint lostPacketSequence = Config.NextSequence(Recv.History.LastOrDefault.Sequence);
        Recv.History.Enqueue(Ack.Lost(lostPacketSequence));
      }

      Recv.History.Enqueue(Ack.Recv(packetSequence, Socket.GetCurrentTime()));
      Recv.PendingAcks += 1;

      return ReadAcks(buffer, o);
    }

    bool ReadAcks(byte[] buffer, int o) {
      var packetAckTime = Blit.ReadU32(buffer, ref o, 2);
      var packetAckSequence = Blit.ReadU32(buffer, ref o, Config.SequenceBytes);

      while (Send.Window.Empty == false) {
        int ackSequenceDistance = Config.Distance(Send.Window.First.Sequence, packetAckSequence);

        if (ackSequenceDistance > 0) {
          break;
        }

        var handle = Send.Window.Dequeue();

        if (ackSequenceDistance <= -Config.AckBits) {
          Lost(handle);
        }
        else {
          var bo = o + (-ackSequenceDistance / 8);
          var bit = 1 << (-ackSequenceDistance % 8);

          if ((buffer[bo] & bit) == bit) {
            Delivered(handle);
          }
          else {
            Lost(handle);
          }
        }

        if (Config.UpdatePing && (ackSequenceDistance == 0) && (packetAckTime > 0)) {
          Connection.UpdatePing(Connection.Socket.GetCurrentTime(), handle.Time, packetAckTime);
        }
      }

      UdpAssert.Assert(((o * 8) + Config.AckBits) == Config.HeaderSizeBits);
      return true;
    }

    void Lost(UdpPipeHandle handle) {
      Connection.Lost(this, handle.Object);
    }

    void Delivered(UdpPipeHandle handle) {
      Connection.Delivered(this, handle.Object);
    }
  }
}
