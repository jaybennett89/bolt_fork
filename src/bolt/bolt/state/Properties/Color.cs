﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UnityEngine;

namespace Bolt {
  internal class NetworkProperty_Color : NetworkProperty {
    public override int BitCount(NetworkObj obj) {
      return 32;
    }

    public override object DebugValue(NetworkObj obj, NetworkStorage storage) {
      return storage.Values[obj[this]].Color;
    }

    public override bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      packet.WriteColorRGBA(storage.Values[obj[this]].Color);
      return true;
    }

    public override void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet) {
      storage.Values[obj[this]].Color = packet.ReadColorRGBA();
    }
  }
}