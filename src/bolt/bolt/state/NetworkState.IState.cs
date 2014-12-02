using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  partial class NetworkState : IState {
    Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();
    Dictionary<string, List<PropertyCallbackSimple>> CallbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

    bool VerifyCallbackPath(string path) {
      if (Meta.CallbackPaths.Contains(path)) {
        return true;
      }

      BoltLog.Error("No callback path '{0}' available on {1}", path, this);
      return false;
    }

    UE.Animator IState.Animator {
      get { return Animators[0]; }
    }

    IEnumerable<UE.Animator> IState.AllAnimators {
      get { return Animators; }
    }

    void IState.AddAnimator(UE.Animator animator) {
      Animators.Add(animator);
    }

    void IState.SetAnimator(UnityEngine.Animator animator) {
      Animators.Clear();
      Animators.Add(animator);
    }

    void IState.AddCallback(string path, PropertyCallback callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          Callbacks[path] = callbacksList = new List<PropertyCallback>(32);
        }

        callbacksList.Add(callback);
      }
    }

    void IState.AddCallback(string path, PropertyCallbackSimple callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallbackSimple> callbacksList;

        if (CallbacksSimple.TryGetValue(path, out callbacksList) == false) {
          CallbacksSimple[path] = callbacksList = new List<PropertyCallbackSimple>(32);
        }

        callbacksList.Add(callback);
      }
    }

    void IState.RemoveCallback(string path, PropertyCallback callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallback> callbacksList;

        if (Callbacks.TryGetValue(path, out callbacksList) == false) {
          callbacksList.Remove(callback);
        }
      }
    }

    void IState.RemoveCallback(string path, PropertyCallbackSimple callback) {
      if (VerifyCallbackPath(path)) {
        List<PropertyCallbackSimple> callbacksList;

        if (CallbacksSimple.TryGetValue(path, out callbacksList) == false) {
          callbacksList.Remove(callback);
        }
      }
    }

    void IState.SetDynamic(string property, object value) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyName == property)) {
          Meta.Properties[i].Property.SetDynamic(this, value);
          return;
        }
      }
    }

    object IState.GetDynamic(string property) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        if ((Meta.Properties[i].OffsetObjects == 0) && (Meta.Properties[i].Property.PropertyName == property)) {
          return Meta.Properties[i].Property.GetDynamic(this);
        }
      }

      throw new ArgumentException(string.Format("Unknown property {0}", property));
    }

    void IDisposable.Dispose() {
    }

    void InvokeCallbacks() {
      if (Frames.first.Changed.IsZero) {
        return;
      }

      // merge into default mask
      PropertyDefaultMask.Combine(Frames.first.Changed);

      var bits = Frames.first.Changed.GetIterator();
      var propertyIndex = -1;

      while (bits.Next(out propertyIndex)) {
        InvokeCallbacksForProperty(propertyIndex);
      }

      if (Entity.Proxies.count > 0) {
        var proxies = Entity.Proxies.GetIterator();

        while (proxies.Next()) {
          proxies.val.Changed.Combine(Frames.first.Changed);
        }
      }

      Frames.first.Changed.ClearAll();
    }

    void InvokeCallbacksForProperty(int propertyIndex) {
      NetworkPropertyInfo pi = Meta.Properties[propertyIndex];

      List<PropertyCallback> callbacks;
      List<PropertyCallbackSimple> callbacksSimple;

      for (int i = 0; i < pi.Paths.Length; ++i) {
        if (Callbacks.TryGetValue(pi.Paths[i], out callbacks)) {
          for (int c = 0; c < callbacks.Count; ++c) {
            callbacks[c](this, pi.Paths[pi.Paths.Length - 1], new ArrayIndices(pi.Indices));
          }
        }

        if (CallbacksSimple.TryGetValue(pi.Paths[i], out callbacksSimple)) {
          for (int c = 0; c < callbacksSimple.Count; ++c) {
            callbacksSimple[c]();
          }
        }
      }
    }

  }
}
