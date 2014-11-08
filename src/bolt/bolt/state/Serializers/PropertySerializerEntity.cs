using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializerSimple {
    public override object GetDebugValue(State state) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(Blit.ReadI32(state.Frames.first.Data, Settings.ByteOffset)));

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override int StateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    protected override bool Pack(byte[] data,  BoltConnection connection, UdpPacket stream) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(Blit.ReadI32(data, Settings.ByteOffset)));

      if ((entity != null) && (connection._entityChannel.ExistsOnRemote(entity) == false)) {
        return false;
      }

      stream.WriteEntity(entity, connection);
      return true;
    }

    protected override void Read(byte[] data, BoltConnection connection, UdpPacket stream) {
      Bolt.Entity entity = stream.ReadEntity(connection);

      if (entity) {
        Blit.PackI32(data, Settings.ByteOffset, entity.InstanceId.Value);
      }
      else {
        Blit.PackI32(data, Settings.ByteOffset, 0);
      }
    }
  }
}
