using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit {
  public class UdpStreamData {
    public byte[] Data;
    public UdpChannelName Channel;
  }

  //public class UdpStreamData {
  //  public byte[] Data { get; private set; }
  //  public UdpDataKey Key { get; private set; }

  //  internal UdpStreamData(byte[] data) {
  //    Key = UdpDataKey.Generate();
  //    Data = data;
  //  }

  //  internal void ClearKey() {
  //    Key = new UdpDataKey();
  //  }
  //}
}
