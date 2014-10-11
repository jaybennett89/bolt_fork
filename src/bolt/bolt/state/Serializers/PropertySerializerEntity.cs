using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  struct PropertySerializerEntityData {
    public bool IsParent;
  }

  class PropertySerializerEntity : PropertySerializerSimple {
    PropertySerializerEntityData PropertyData;

    public PropertySerializerEntity(StatePropertyMetaData meta) : base(meta) { }
    public PropertySerializerEntity(EventPropertyMetaData meta) : base(meta) { }
    public PropertySerializerEntity(CommandPropertyMetaData meta) : base(meta) { }

    public void SetPropertyData(PropertySerializerEntityData propertyData) {
      PropertyData = propertyData;
    }

    public override object GetDebugValue(State state) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset)));

      if (entity) {
        return entity.ToString();
      }

      return "NULL";
    }

    public override int StateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(Blit.ReadI32(data, offset)));

      if ((entity != null) && (connection._entityChannel.ExistsOnRemote(entity) == false)) {
        return false;
      }

      stream.WriteEntity(entity, connection);
      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Bolt.Entity entity = stream.ReadEntity(connection);

      if (entity) {
        Blit.PackI32(data, offset, entity.InstanceId.Value);
      }
      else {
        Blit.PackI32(data, offset, 0);
      }
    }
  }
}
