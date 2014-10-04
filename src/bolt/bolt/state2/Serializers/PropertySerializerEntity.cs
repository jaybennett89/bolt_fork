using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializer {
    public PropertySerializerEntity(StatePropertyMetaData info)
      : base(info) {
    }

    public override int StateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      Bolt.EntityObject entity = BoltCore.FindEntity(new InstanceId(frame.Data.ReadI32(StateData.ByteOffset)));

      if (stream.WriteBool(entity != null)) {
        if (connection._entityChannel.ExistsOnRemote(entity)) {
          return false;
        }

        stream.WriteNetworkId(connection._entityChannel.GetNetworkId(entity));
      }

      return true;
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      int instanceId = 0;

      if (stream.ReadBool()) {
        instanceId = connection._entityChannel.GetIncommingEntity(stream.ReadNetworkId()).InstanceId.Value;
      }

      frame.Data.PackI32(StateData.ByteOffset, instanceId);
    }
  }
}
