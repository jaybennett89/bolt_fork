using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit {
  class UdpStreamOp {
    public readonly UInt64 Key;
    public readonly byte[] Data;
    public readonly UdpChannelName Channel;

    public int BlockCurrent;
    public int BlockSize;
    public int BlockCount;

    public uint CRC;
    public uint DoneTime;

    public ulong Pending;
    public ulong Delivered;

    public bool IsDone {
      get { return DoneTime > 0; }
    }

    public int BlocksRemaining {
      get { return BlockCount - BlockCurrent; }
    }

    public UdpStreamOp(UInt64 key, UdpChannelName channel, byte[] data) {
      Key = key;
      Data = data;
      Channel = channel;
    }
  }

}
