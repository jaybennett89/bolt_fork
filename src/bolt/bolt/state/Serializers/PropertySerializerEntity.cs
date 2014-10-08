using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;

namespace Bolt {
  struct PropertySerializerEntityData {
    public bool IsParent;
  }

  class PropertySerializerEntity : PropertySerializer {
    PropertySerializerEntityData PropertyData;

    public PropertySerializerEntity(StatePropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerEntity(EventPropertyMetaData meta)
      : base(meta) {
    }

    public PropertySerializerEntity(CommandPropertyMetaData meta)
      : base(meta) {
    }

    public void SetPropertyData(PropertySerializerEntityData propertyData) {
      PropertyData = propertyData;
    }

    public override void DisplayDebugValue(State state) {
      BoltGUI.Label(Blit.ReadI32(state.Frames.first.Data, StateData.ByteOffset));
    }

    public override int StateBits(State state, State.Frame frame) {
      return EntityProxy.ID_BIT_COUNT + 1;
    }

    public override void OnSimulateAfter(State state) {
      if (PropertyData.IsParent) {
        InstanceId id = new InstanceId(state.Frames.first.Data.ReadI32(StateData.ByteOffset));
        Entity en = BoltCore.FindEntity(id);

        if (en) {
          state.Entity.UnityObject.transform.parent = en.UnityObject.transform.parent;
        }
        else {
          state.Entity.UnityObject.transform.parent = null;
        }
      }
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      Bolt.Entity entity = BoltCore.FindEntity(new InstanceId(frame.Data.ReadI32(StateData.ByteOffset)));

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
