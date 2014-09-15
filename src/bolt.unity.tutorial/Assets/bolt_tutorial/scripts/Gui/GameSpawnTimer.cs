using UnityEngine;
using System.Collections;

public class GameSpawnTimer : BoltCallbacks {
  BoltEntity me;
  IPlayerState meState;

  [SerializeField]
  TypogenicText timer;

  public override void ControlOfEntityGained(BoltEntity arg) {
    if (arg.boltSerializer is PlayerSerializer) {
      me = arg;
      meState = me.GetBoltState<IPlayerState>();
    }
  }

  public override void ControlOfEntityLost(BoltEntity arg) {
    if (arg.boltSerializer is PlayerSerializer) {
      me = null;
      meState = null;
    }
  }

  void Update() {
    // lock in middle of screen
    transform.position = Vector3.zero;

    // update timer
    if (me && meState != null) {
      if (meState.dead) {
        timer.Set(Mathf.Max(0, (meState.respawnFrame - BoltNetwork.frame) / BoltNetwork.framesPerSecond).ToString());
      }
      else {
        timer.Set("");
      }
    }
    else {
      timer.Set("");
    }
  }
}
