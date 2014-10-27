using System;
using UdpKit;

namespace Bolt {
  internal enum PropertyModes {
    State = 0,
    Command = 1,
    Event = 2
  }

  abstract class PropertySerializer {
    public PropertySettings Settings;
    public PropertyStateSettings StateSettings;
    public PropertyCommandSettings CommandSettings;
    public PropertySmoothingSettings SmoothingSettings;

    public void AddSettings(PropertySettings settings) {
      Settings = settings;
    }

    public void AddSettings(PropertyStateSettings stateSettings) {
      StateSettings = stateSettings;
    }

    public void AddSettings(PropertyCommandSettings commandSettings) {
      CommandSettings = commandSettings;
    }

    public void AddSettings(PropertySmoothingSettings smoothingSettings) {
      SmoothingSettings = smoothingSettings;
    }

    public virtual object GetDebugValue(State state) { return null; }

    public virtual int StateBits(State state, State.Frame frame) { throw new NotSupportedException(); }
    public virtual bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual bool EventPack(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void EventRead(Event data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }

    public virtual void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    public virtual void CommandSmooth(byte[] from, byte[] to, byte[] into, float t) { }

    public virtual void OnInit(State state) { }
    public virtual void OnSimulateBefore(State state) { }
    public virtual void OnSimulateAfter(State state) { }
    public virtual void OnRender(State state, State.Frame frame) { }
    public virtual void OnParentChanged(State state, Entity newParent, Entity oldParent) { }
  }

  abstract class PropertySerializerSimple : PropertySerializer {
    public override bool EventPack(Event data, BoltConnection connection, UdpStream stream) {
      return Pack(data.Data, connection, stream);
    }

    public override void EventRead(Event data, BoltConnection connection, UdpStream stream) {
      Read(data.Data, connection, stream);
    }

    public override bool StatePack(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      return Pack(frame.Data, connection, stream);
    }

    public override void StateRead(State state, State.Frame frame, BoltConnection connection, UdpStream stream) {
      Read(frame.Data, connection, stream);
    }

    public override void CommandPack(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) {
      Pack(data, connection, stream);
    }

    public override void CommandRead(Command cmd, byte[] data, BoltConnection connection, UdpStream stream) {
      Read(data, connection, stream);
    }

    protected virtual bool Pack(byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
    protected virtual void Read(byte[] data, BoltConnection connection, UdpStream stream) { throw new NotSupportedException(); }
  }

  abstract class PropertySerializerMecanim : PropertySerializerSimple {
    protected PropertyMecanimSettings MecanimSettings;

    protected bool ShouldPullDataFromMecanim(State state) {
      return MecanimSettings.Direction == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl);
    }

    public void AddSettings(PropertyMecanimSettings mecanimSettings) {
      MecanimSettings = mecanimSettings;
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
      state.Frames.first.Data.PackF32(Settings.ByteOffset, state.Animator.GetLayerWeight(MecanimSettings.Layer));
    }

    void PushMecanimLayer(State state) {
      state.Animator.SetLayerWeight(MecanimSettings.Layer, state.Frames.first.Data.ReadF32(Settings.ByteOffset));
    }
  }
}
