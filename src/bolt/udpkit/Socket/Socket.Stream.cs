using System;
using System.Collections.Generic;

using System.Text;

namespace UdpKit {
  partial class UdpSocket {
    //public UdpStreamData StreamDataCreate(byte[] bytes) {
    //  UdpStreamData data = new UdpStreamData(bytes);

    //  // add to stream data looku p
    //  streamData.Add(data.Key, data);

    //  return data;
    //}

    //public void StreamDataRemove(UdpStreamData data) {
    //  if (data.Key.IsZero) {
    //    UdpLog.Error("Can't unregister data with zero key");
    //    return;
    //  }

    //  if (streamData.Remove(data.Key) == false) {
    //    UdpLog.Error("Stream data not found in lookup table");
    //  }

    //  data.ClearKey();
    //}

    //public bool StreamDataExists(UdpDataKey key) {
    //  return streamData.ContainsKey(key);
    //}

    //public UdpStreamData StreamDataFind(UdpDataKey key) {
    //  UdpStreamData data;

    //  if (streamData.TryGetValue(key, out data)) {
    //    return data;
    //  }

    //  return null;
    //}

    public UdpChannelName StreamChannelCreate(string name, UdpChannelMode mode, int priority) {
      UdpAssert.Assert(name != null);
      UdpAssert.Assert(priority > 0);

      UdpChannelConfig config;
      
      config = new UdpChannelConfig();
      config.Mode = mode;
      config.Priority = priority;
      config.ChannelName.Name = name;
      config.ChannelName.Id = ++channelIdCounter;

      UdpEvent ev = new UdpEvent();
      ev.Type = UdpEvent.INTERNAL_STREAM_CREATECHANNEL;
      ev.ChannelConfig = config;

      Raise(ev);

      return config.ChannelName;
    }
  }
}
