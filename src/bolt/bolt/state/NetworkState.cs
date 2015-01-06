using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdpKit;
using UE = UnityEngine;

namespace Bolt {
  internal abstract partial class NetworkState : NetworkObj_Root {
#if DEBUG
    internal float MecanimWarningTimeout = 0;
#endif

    internal Entity Entity;
    internal List<UE.Animator> Animators = new List<UE.Animator>();
    internal new NetworkState_Meta Meta;

    internal BitSet PropertyDefaultMask = new BitSet();
    internal Priority[] PropertyPriorityTemp;

    internal BoltDoubleList<NetworkStorage> Frames = new BoltDoubleList<NetworkStorage>();

    public UE.Animator Animator {
      get { return Animators.Count > 0 ? Animators[0] : null; }
    }

    internal sealed override NetworkStorage Storage {
      get { return Frames.first; }
    }

    internal NetworkState(NetworkState_Meta meta)
      : base(meta) {
      Meta = meta;
      Meta.PropertyIdBits = 32;
      Meta.PacketMaxPropertiesBits = 8;
    }

    public void SetAnimator(UE.Animator animator) {
      Animators.Clear();

      if (animator) {
        Animators.Add(animator);
      }
    }

    public void AddAnimator(UE.Animator animator) {
      if (animator) {
        Animators.Add(animator);
      }
    }
  }
}
