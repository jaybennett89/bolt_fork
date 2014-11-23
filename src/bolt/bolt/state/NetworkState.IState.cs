using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UE = UnityEngine;

namespace Bolt {
  partial class NetworkState : IState {
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
    }

    void IState.AddCallback(string path, PropertyCallbackSimple callback) {
    }

    void IState.RemoveCallback(string path, PropertyCallback callback) {
    }

    void IState.RemoveCallback(string path, PropertyCallbackSimple callback) {
    }

    void IState.SetDynamic(string property, object value) {
    }

    void IDisposable.Dispose() {
    }
  }
}
