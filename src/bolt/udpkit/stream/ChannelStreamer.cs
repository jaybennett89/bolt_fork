using System;
using System.Collections.Generic;

namespace UdpKit {
  class UdpChannelStreamer {
    ulong SendKeyCounter;
    ulong RecvKeyCounter;

    UdpConnection Connection;

    public int Priority;
    public UdpStreamChannel Channel;
    public Dictionary<UInt64, UdpStreamOp> OutgoingData;
    public Dictionary<UInt64, UdpStreamOp> IncommingData;

    public UdpChannelStreamer(UdpConnection connection, UdpStreamChannel channel) {
      UdpAssert.Assert(channel != null);
      UdpAssert.Assert(connection != null);

      Channel = channel;
      Priority = channel.Config.Priority;
      Connection = connection;

      OutgoingData = new Dictionary<UInt64, UdpStreamOp>();
      IncommingData = new Dictionary<UInt64, UdpStreamOp>();
    }

    public void Queue(byte[] data) {
      UdpStreamOp op = new UdpStreamOp(++SendKeyCounter, Channel.Name, data);

#if DEBUG
      InitOp(op, DamienG.Security.Cryptography.Crc32.Compute(data));
#else
      InitOp(op, 0);
#endif

      // add to outgoing data
      OutgoingData.Add(op.Key, op);
    }

    public void OnBlockLost(UdpStreamOp op, int block) {
      // no point in tracking this on an unreliable channel
      if (Channel.Config.Mode == UdpChannelMode.Unreliable) {
        return;
      }

      var bit = block - op.BlockCurrent;

      UdpAssert.Assert(bit >= 0 && bit < 64);

      op.Pending &= ~(1UL << bit);
    }

    public void OnBlockDelivered(UdpStreamOp op, int block) {
      // no point in tracking this on an unreliable channel
      if (Channel.Config.Mode == UdpChannelMode.Unreliable) {
        return;
      }

      var bit = block - op.BlockCurrent;

      UdpAssert.Assert(bit >= 0 && bit < 64);

      op.Pending &= ~(1UL << bit);
      op.Delivered |= (1UL << bit);

      while ((op.Delivered & 1UL) == 1UL) {
        op.Pending >>= 1;
        op.Delivered >>= 1;
        op.BlockCurrent += 1;
      }

      if (op.BlockCurrent == op.BlockCount) {
        op.DoneTime = Connection.Socket.GetCurrentTime();
      }
    }

    public bool TrySend() {
      if (OutgoingData.Count > 0) {
        foreach (var d in OutgoingData.Values) {
          if (d.IsDone) {
            continue;
          }

          var remaining = Math.Min(d.BlocksRemaining, 64);

          for (var r = 0; r < remaining; ++r) {
            var b = 1UL << r;

            // if this block is not delivered and not pending
            if (((d.Delivered & b) == 0UL) && ((d.Pending & b) == 0UL)) {

              // try to send the block of this data on this channel to the remote end
              if (SendBlock(d, d.BlockCurrent + r)) {
                // if this is an unreliable channel we just remove the item right away
                if (Channel.Config.Mode == UdpChannelMode.Unreliable) {
                  OutgoingData.Remove(d.Key);
                }

                // if not, track pending delivery
                else {
                  d.Pending |= b;
                }

                // return to caller that we succeeded
                return true;
              }
              else {
                return false;
              }
            }
          }
        }
      }

      return false;
    }

    public void OnBlockReceived(byte[] buffer, int bytes, int o) {
      var dataKey = Blit.ReadU64(buffer, ref o);
      var dataLength = Blit.ReadI32(buffer, ref o);

#if DEBUG
      var dataCRC = Blit.ReadU32(buffer, ref o);
#endif


      UdpStreamOp op = null;

      if (IncommingData.TryGetValue(dataKey, out op) == false) {
        op = new UdpStreamOp(dataKey, Channel.Name, new byte[dataLength]);

#if DEBUG
        InitOp(op, dataCRC);
#else
        InitOp(op, 0);
#endif

        IncommingData.Add(op.Key, op);
      }

      if (op.IsDone) {
        return;
      }

      RecvBlock(op, buffer, bytes, o);
    }

    void InitOp(UdpStreamOp op, uint crc) {
      op.CRC = crc;
      op.BlockSize = (Connection.Socket.StreamPipeConfig.DatagramSize - Connection.StreamPipe.Config.HeaderSize);
      op.BlockCount = op.Data.Length / op.BlockSize;

      if ((op.Data.Length % op.BlockSize) != 0) {
        UdpAssert.Assert((op.Data.Length % op.BlockSize) < op.BlockSize);
        op.BlockCount += 1;
      }
    }

    bool SendBlock(UdpStreamOp op, int block) {
      var buffer = Connection.Socket.GetRecvBuffer();
      var streamBlock = new UdpStreamOpBlock { Op = op, Number = block };

      if (Connection.StreamPipe.WriteHeader(buffer, streamBlock)) {
        var o = Connection.StreamPipe.Config.HeaderSize;
        var l = Math.Min(op.BlockSize, op.Data.Length - (block * op.BlockSize));

        Blit.PackI32(buffer, ref o, Channel.Name.Id);
        Blit.PackU64(buffer, ref o, op.Key);
        Blit.PackI32(buffer, ref o, op.Data.Length);

#if DEBUG
        Blit.PackU32(buffer, ref o, op.CRC);
#endif

        Blit.PackI32(buffer, ref o, block);
        Blit.PackBytes(buffer, ref o, op.Data, block * op.BlockSize, l);

        // send to remote
        Connection.Socket.Send(Connection.RemoteEndPoint, buffer, o);

        return true;
      }

      return false;
    }

    void RecvBlock(UdpStreamOp op, byte[] buffer, int bytes, int o) {
      var block = Blit.ReadI32(buffer, ref o);
      if (block < op.BlockCurrent) {
        // already received
        return;
      }

      var offset = block - op.BlockCurrent;
      if ((offset < 0) || (offset >= 64)) {
        Connection.ConnectionError(UdpConnectionError.InvalidBlockNumber);
        return;
      }

      var bit = 1UL << offset;
      if (bit == (op.Delivered & bit)) {
        // already received
        return;
      }

      // copy from buffer into op data
      Array.Copy(buffer, o, op.Data, op.BlockSize * block, bytes - o);

      // mark as delivered;
      op.Delivered |= bit;

      // update delivered blocks
      while ((op.Delivered & 1UL) == 1UL) {
        op.Delivered >>= 1;
        op.BlockCurrent += 1;
      }

      if (op.BlockCurrent == op.BlockCount) {
        UdpAssert.Assert(op.DoneTime == 0);

        op.DoneTime = Connection.Socket.GetCurrentTime();

#if DEBUG
        var crc = DamienG.Security.Cryptography.Crc32.Compute(op.Data);
        if (crc != op.CRC) {
          UdpLog.Error("CRC did not match {0} (expected) / {1} (calculated)", op.CRC, crc);
        }
#endif

        UdpEvent ev;

        ev = new UdpEvent();
        ev.Type = UdpEvent.PUBLIC_STREAM_DATARECEIVED;
        ev.Connection = Connection;
        ev.StreamData = new UdpStreamData { Channel = Channel.Name, Data = op.Data };

        Connection.Socket.Raise(ev);
      }
      else {
        if (Channel.Config.Mode == UdpChannelMode.Unreliable) {
          UdpLog.Error("Received partial block on unreliable {0}", Channel.Name);
        }
      }
    }
  }
}