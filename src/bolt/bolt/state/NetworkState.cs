using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal abstract partial class NetworkState : NetworkObj_Root {
    internal Entity Entity;
    internal List<UE.Animator> Animators = new List<UE.Animator>();
    internal new NetworkState_Meta Meta;

    internal BitSet PropertyDefaultMask;
    internal Priority[] PropertyPriorityTemp;

    internal BoltDoubleList<NetworkStorage> Frames = new BoltDoubleList<NetworkStorage>();
    internal Dictionary<string, List<PropertyCallback>> Callbacks = new Dictionary<string, List<PropertyCallback>>();
    internal Dictionary<string, List<PropertyCallbackSimple>> CallbacksSimple = new Dictionary<string, List<PropertyCallbackSimple>>();

    public UE.Animator Animator {
      get { return Animators[0]; }
    }

    internal sealed override NetworkStorage Storage {
      get { return Frames.first; }
    }

    internal NetworkState(NetworkState_Meta meta)
      : base(meta) {
      Meta = meta;
    }

    public void SetAnimator(UE.Animator animator) {
      Animators.Clear();
      Animators.Add(animator);
    }

    public void AddAnimator(UE.Animator animator) {
      Animators.Add(animator);
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

    }
  }
}
