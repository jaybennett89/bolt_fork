using System;
using System.Collections.Generic;
using UdpKit;

namespace Bolt {
  internal enum PropertyModes {
    State = 0,
    Command = 1,
    Event = 2
  }

  abstract class PropertySerializer {
    public PropertySerializerSettings Settings;
    public PropertyCommandSettings CommandSettings;
    public PropertySmoothingSettings SmoothingSettings;

    public void AddSerializerSettings(string propertyName, int propertyPriority, PropertyModes propertyMode) {
      Settings.PropertyName = propertyName;
      Settings.PropertyMode = propertyMode;
      Settings.PropertyPriority = propertyPriority;
      Settings.PropertyPaths = new List<string>();
    }

    public void AddCommandSettings(bool smoothCorrections) {
      CommandSettings.SmoothCorrections = smoothCorrections;
    }

    public void AddSettings(PropertySmoothingSettings smoothingSettings) {
      SmoothingSettings = smoothingSettings;
    }

    public virtual object GetDebugValue(State state) { return null; }
    public virtual void SetDynamic(NetworkFrame frame, object value) { throw new NotSupportedException(); }

    public virtual int StateBits(State state, NetworkFrame frame) { throw new NotSupportedException(); }
    public virtual bool StatePack(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
    public virtual void StateRead(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }

    public virtual bool EventPack(NetworkEvent data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
    public virtual void EventRead(NetworkEvent data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }

    public virtual void CommandPack(Command cmd, NetworkValue[] data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
    public virtual void CommandRead(Command cmd, NetworkValue[] data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
    public virtual void CommandSmooth(NetworkValue[] from, NetworkValue[] to, NetworkValue[] into, float t) { }

    public virtual void CreateObjects(State state) { }

    public virtual void OnInit(State state) { }
    public virtual void OnSimulateBefore(State state) { }
    public virtual void OnSimulateAfter(State state) { }
    public virtual void OnRender(State state, NetworkFrame frame) { }
    public virtual void OnParentChanged(State state, Entity newParent, Entity oldParent) { }

    internal void Verify(Type type, string name, int serializer, int storage, int objects) {
      Assert.True(ReferenceEquals(this.GetType(), type), "ReferenceEquals(this.GetType(), type)");
      Assert.True(this.Settings.PropertyName == name, "{0} == {1} /* this.Settings.PropertyName == name */", this.Settings.PropertyName, name);
      Assert.True(this.Settings.OffsetObjects == objects, "this.Settings.OffsetObjects == objects");
      Assert.True(this.Settings.OffsetStorage == storage, "this.Settings.OffsetStorage == storage");
      Assert.True(this.Settings.OffsetSerializers == serializer, "this.Settings.OffsetSerializers == serializer");
    }
  }

  abstract class PropertySerializerSimple : PropertySerializer {
    public override bool EventPack(NetworkEvent data, BoltConnection connection, UdpPacket stream) {
      //return Pack(data.Data, connection, stream);
      return true;
    }

    public override void EventRead(NetworkEvent data, BoltConnection connection, UdpPacket stream) {
      //Read(data.Data, connection, stream);
    }

    public override bool StatePack(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) {
      return Pack(frame.Storage, connection, stream);
    }

    public override void StateRead(State state, NetworkFrame frame, BoltConnection connection, UdpPacket stream) {
      Read(frame.Storage, connection, stream);
    }

    public override void CommandPack(Command cmd, NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      Pack(data, connection, stream);
    }

    public override void CommandRead(Command cmd, NetworkValue[] data, BoltConnection connection, UdpPacket stream) {
      Read(data, connection, stream);
    }

    protected virtual bool Pack(NetworkValue[] data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
    protected virtual void Read(NetworkValue[] data, BoltConnection connection, UdpPacket stream) { throw new NotSupportedException(); }
  }

  abstract class PropertySerializerMecanim : PropertySerializerSimple {
    protected PropertyMecanimSettings MecanimSettings;

    protected bool ShouldPullDataFromMecanim(State state) {
      return MecanimSettings.Direction == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl);
    }

    public void AddMecanimSettings(MecanimMode mode, MecanimDirection direction, float damping, int layer) {
      MecanimSettings.Mode = mode;
      MecanimSettings.Direction = direction;
      MecanimSettings.Damping = damping;
      MecanimSettings.Layer = layer;
    }

    public override void OnSimulateAfter(State state) {
      if (MecanimSettings.Enabled) {
        if (state.Animator) {
          if (MecanimSettings.Mode == MecanimMode.LayerWeight) {
            if (ShouldPullDataFromMecanim(state)) {
              PullMecanimLayer(state);
            }
            else {
              PushMecanimLayer(state);
            }
          }
          else {
            if (ShouldPullDataFromMecanim(state)) {
              PullMecanimValue(state);
            }
            else {
              PushMecanimValue(state);
            }
          }
        }
        else {
          BoltLog.Warn("You have not assigned a mecanim animator to the state on {0}, but you have properties set to use mecanim", state.Entity.UnityObject.gameObject.name);
        }
      }
    }

    protected virtual void PullMecanimValue(State state) { }
    protected virtual void PushMecanimValue(State state) { }

    void PullMecanimLayer(State state) {
      state.CurrentFrame.Storage[Settings.OffsetStorage].Float0 = state.Animator.GetLayerWeight(MecanimSettings.Layer);
    }

    void PushMecanimLayer(State state) {
      state.Animator.SetLayerWeight(MecanimSettings.Layer, state.CurrentFrame.Storage[Settings.OffsetStorage].Float0);
    }
  }
}
