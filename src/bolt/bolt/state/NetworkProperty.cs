using System;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal abstract class NetworkProperty {
    public Int32 OffsetStorage;
    public Int32 OffsetProperties;

    public int PropertyFilters;
    public String PropertyName;
    public Int32 PropertyPriority;
    public NetworkObj_Meta PropertyMeta;

    public PropertyInterpolationSettings Interpolation;

    public virtual bool AllowCallbacks { get { return true; } }
    public virtual bool WantsOnRender { get { return false; } }
    public virtual bool WantsOnSimulateAfter { get { return false; } }
    public virtual bool WantsOnSimulateBefore { get { return false; } }

    public void Settings_Property(string name, int priority, int filters) {
      PropertyName = name;
      PropertyFilters = filters;
      PropertyPriority = UE.Mathf.Clamp(priority, 1, 100);
    }

    public void Settings_Offsets(Int32 properties, Int32 storage) {
      OffsetStorage = storage;
      OffsetProperties = properties;
    }

    public void Settings_Interpolation(float snapMagnitude) {
      Interpolation.Enabled = true;
      Interpolation.SnapMagnitude = snapMagnitude;
    }

    public abstract bool Write(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet);
    public abstract void Read(BoltConnection connection, NetworkObj obj, NetworkStorage storage, UdpPacket packet);

    public virtual object DebugValue(NetworkObj obj, NetworkStorage storage) { return "NONE"; }
    public virtual int BitCount(NetworkObj obj) { return -1; }

    public virtual void OnInit(NetworkObj obj) { }
    public virtual void OnRender(NetworkObj obj) { }
    public virtual void OnSimulateBefore(NetworkObj obj) { }
    public virtual void OnSimulateAfter(NetworkObj obj) { }
    public virtual void OnParentChanged(NetworkObj obj, Entity newParent, Entity oldParent) { }

    public virtual void SmoothCommandCorrection(NetworkObj obj, NetworkStorage from, NetworkStorage to, NetworkStorage storage, float t) { }
  }

  internal abstract class NetworkProperty_Mecanim : NetworkProperty {
    public MecanimMode MecanimMode;
    public MecanimDirection MecanimDirection;
    public float MecanimDamping;
    public int MecanimLayer;

    public override bool WantsOnSimulateAfter {
      get { return true; }
    }

    public void Settings_Mecanim(MecanimMode mode, MecanimDirection direction, float damping, int layer) {
      MecanimMode = mode;
      MecanimDirection = direction;
      MecanimDamping = damping;
      MecanimLayer = layer;
    }

    public override void OnSimulateAfter(NetworkObj obj) {
      NetworkState state = (NetworkState)obj.Root;

      if (MecanimMode != MecanimMode.Disabled) {
        if (state.Animator) {
          if (MecanimMode == MecanimMode.LayerWeight) {
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

    protected bool ShouldPullDataFromMecanim(NetworkState state) {
      return MecanimDirection == MecanimDirection.UsingAnimatorMethods && (state.Entity.IsOwner || state.Entity.HasPredictedControl);
    }

    protected virtual void PullMecanimValue(NetworkState state) { }
    protected virtual void PushMecanimValue(NetworkState state) { }

    void PullMecanimLayer(NetworkState state) {
      state.Storage.Values[state[this]].Float0 = state.Animator.GetLayerWeight(MecanimLayer);
    }

    void PushMecanimLayer(NetworkState state) {
      for (int i = 0; i < state.Animators.Count; ++i) {
        state.Animators[i].SetLayerWeight(MecanimLayer, state.Storage.Values[state[this]].Float0);
      }
    }
  }
}
