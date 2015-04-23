using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  partial class UdpConnection {
    public void StreamSetBandwidth(int bytesPerSecond) {
      Socket.Raise(new UdpEventStreamSetBandwidth { Connection = this, BytesPerSecond = bytesPerSecond });
    }

    internal void OnStreamSetBandwidth(int byteRate) {
      StreamSendInterval = (uint)(byteRate / Socket.StreamPipeConfig.DatagramSize);
      UdpLog.Debug("{0} set to stream {1} packets at {2} bytes each every second", this, StreamSendInterval, Socket.StreamPipeConfig.DatagramSize);
    }

    public void StreamBytes(UdpChannelName channel, byte[] data) {
      Socket.Raise(new UdpEventStreamQueue { Connection = this, StreamOp = new UdpStreamOp(0, channel, data) });
    }

    internal void OnStreamQueue(UdpStreamChannel c, UdpStreamOp op) {
      UdpChannelStreamer s;

#if DEBUG
      op.CRC = DamienG.Security.Cryptography.Crc32.Compute(op.Data);
#endif

      if (StreamChannels.TryGetValue(c.Name, out s) == false) {
        StreamChannels[c.Name] = s = new UdpChannelStreamer(this, c);
      }

      if (s.Channel.Config.Mode == UdpChannelMode.Unreliable) {
        UdpLog.Info("Sending {0} bytes unreliably to {1}", op.Data.Length, this.RemoteEndPoint);

        int maxUnreliableSize = (StreamPipe.Config.DatagramSize - StreamPipe.Config.HeaderSize);

        if (op.Data.Length > maxUnreliableSize) {
          UdpLog.Error("Can't queue unreliable data streams larger than {0}", maxUnreliableSize);
          return;
        }

        var o = 0;
        var buffer = Socket.GetRecvBuffer();

        Blit.PackByte(buffer, ref o, UdpPipe.PIPE_STREAM_UNRELIABLE);
        Blit.PackI32(buffer, ref o, c.Name.Id);
        Blit.PackBytesPrefix(buffer, ref o, op.Data);

        Socket.Send(RemoteEndPoint, buffer, o);
      }
      else {
        s.Queue(op.Data);
      }
    }

    internal void OnStreamReceived_Unreliable(byte[] buffer, int size) {
      var o = 1;
      var channelId = Blit.ReadI32(buffer, ref o);
      var data = Blit.ReadBytesPrefix(buffer, ref o);

      UdpStreamChannel channel;

      if (Socket.FindChannel(channelId, out channel)) {

        UdpEvent ev;

        ev = new UdpEvent();
        ev.Type = UdpEvent.PUBLIC_STREAM_DATARECEIVED;
        ev.Object0 = this;
        ev.Object1 = new UdpStreamData { Channel = channel.Name, Data = data };

        this.Socket.Raise(ev);
      }
    }

    internal void OnStreamReceived(byte[] buffer, int bytes) {
      RecvTime = Socket.GetCurrentTime();

      if (StreamPipe.ReadHeader(buffer, bytes)) {
        if (bytes > StreamPipe.Config.HeaderSize) {
          var o = StreamPipe.Config.HeaderSize;
          var channelId = Blit.ReadI32(buffer, ref o);

          UdpStreamChannel channel;

          if (Socket.FindChannel(channelId, out channel)) {
            UdpLog.Info("Received {0} bytes unreliably from {1}", bytes, this.RemoteEndPoint);

            UdpChannelStreamer s;

            if (StreamChannels.TryGetValue(new UdpChannelName(channelId), out s) == false) {
              StreamChannels.Add(channel.Name, s = new UdpChannelStreamer(this, channel));
            }

            s.OnBlockReceived(buffer, bytes, o);
          }
          else {
            ConnectionError(UdpConnectionError.UnknownStreamChannel, channelId.ToString());
          }
        }
      }
    }

    void OnStreamLost(UdpStreamOpBlock block) {
      UdpChannelStreamer s;

      if (StreamChannels.TryGetValue(block.Op.Channel, out s)) {
        s.OnBlockLost(block.Op, block.Number);
      }
    }

    void OnStreamDelivered(UdpStreamOpBlock block) {
      UdpChannelStreamer s;

      if (StreamChannels.TryGetValue(block.Op.Channel, out s)) {
        s.OnBlockDelivered(block.Op, block.Number);
      }
    }

    void SendStream() {
      // make sure we can send data
      if (IsConnected == false) {
        return;
      }

      // if we don't have any channels don't do anything
      if (StreamChannels.Count == 0) {
        return;
      }

      var channels = new List<UdpChannelStreamer>();

      foreach (var s in StreamChannels.Values) {
        if (s.OutgoingData.Count > 0) {
          channels.Add(s);
        }
      }

      // do we have any channels with pending data
      if (channels.Count == 0) {
        return;
      }

      // if we don't have any channels with a priority of > 0, then reset priority on all of them
      if (channels.FindIndex(x => x.Priority > 0) == -1) {
        foreach (var s in channels) {
          s.Priority = s.Channel.Config.Priority;
        }
      }

      // order by priority, in reverse, and try to send from each one in order
      channels.Sort((a, b) => b.Priority.CompareTo(a.Priority));


      // try to send from each one
      foreach (UdpChannelStreamer s in channels) {
        if (s.Priority == 0) {
          s.Priority = s.Channel.Config.Priority;
        }

        if (s.TrySend()) {
          // if we managed to send, decrease priority
          s.Priority -= 1;

          // make sure this never goes below zero
          UdpAssert.Assert(s.Priority >= 0);

          // we're done after one channel sent data this time
          return;
        }
      }
    }
  }
}
