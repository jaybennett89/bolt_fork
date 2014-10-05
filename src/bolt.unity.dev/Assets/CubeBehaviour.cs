using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  public override void Attached() {
    BoltLog.Info("HAS CONTROL:" + entity.hasControl);

    state.SetAnimator(GetComponent<Animator>());
    state.Transform.Simulate = transform;
  }
}
