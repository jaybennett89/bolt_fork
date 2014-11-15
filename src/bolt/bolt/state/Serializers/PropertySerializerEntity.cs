﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      Bolt.Entity entity = BoltCore.FindEntity(state.Frames.first.Data.ReadNetworkId(Settings.ByteOffset));

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override int StateBits(State state, State.Frame frame) {
      return 8 * 8;
    }

    protected override bool Pack(byte[] data,  BoltConnection connection, UdpPacket stream) {
      Bolt.Entity entity = BoltCore.FindEntity(data.ReadNetworkId(Settings.ByteOffset));

      if ((entity != null) && (connection._entityChannel.ExistsOnRemote(entity) == false)) {
        return false;
      }

      stream.WriteEntity(entity);
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Bolt.Entity entity = stream.ReadEntity();

      if (entity) {
        data.PackNetworkId(Settings.ByteOffset, entity.NetworkId);
      }
      else {
        data.PackNetworkId(Settings.ByteOffset, default(NetworkId));
      }
    }
  }
}
