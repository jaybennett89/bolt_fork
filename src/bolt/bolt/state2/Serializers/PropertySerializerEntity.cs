using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  class PropertySerializerEntity : PropertySerializer {
    public PropertySerializerEntity(PropertyMetaData info)
      : base(info) {
    }

    public override int CalculateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    public override bool Pack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      Bolt.EntityObject entity = BoltCore.FindEntity(new InstanceId(frame.Data.ReadI32(MetaData.ByteOffset)));

      if (stream.WriteBool(entity != null)) {
        if (connection._entityChannel.ExistsOnRemote(entity)) {
          return false;
        }

        stream.WriteNetworkId(connection._entityChannel.GetNetworkId(entity));
      }

      return true;
    }
    public override void Read(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      int instanceId = 0;

      if (stream.ReadBool()) {
        instanceId = connection._entityChannel.GetIncommingEntity(stream.ReadNetworkId()).InstanceId.Value;
      }

      frame.Data.PackI32(MetaData.ByteOffset, instanceId);
    }
  }
}
