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

    public override void DisplayDebugValue(State state) {
      BoltGUI.Label(Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset));
    }

    public override int StateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    protected override bool Pack(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(Blit.ReadI32(data, offset)));

      if (stream.WriteBool(entity != null)) {
        if (connection._entityChannel.ExistsOnRemote(entity)) {
          return false;
        }

        stream.WriteNetworkId(connection._entityChannel.GetNetworkId(entity));
      }

      return true;
    }

    protected override void Read(byte[] data, int offset, BoltConnection connection, UdpStream stream) {
      int instanceId = 0;

      if (stream.ReadBool()) {
        instanceId = connection._entityChannel.GetIncommingEntity(stream.ReadNetworkId()).InstanceId.Value;
      }

      Blit.PackI32(data, offset, instanceId);
    }
  }
}
