using UnityEngine;
using System.Collections;

[BoltGlobalBehaviour("Level1")]
public class PlayerCallbacks : BoltCallbacks {
  public override void SceneLoadLocalDone(string map) {
    // ui
    GameUI.Instantiate();

    // camera
    PlayerCamera.Instantiate();
  }

  public override void ControlOfEntityGained(BoltEntity arg) {
    if (arg.boltSerializer is PlayerSerializer) {
      // add audio listener to our character
      arg.gameObject.AddComponent<AudioListener>();

      // set camera callbacks
      PlayerCamera.instance.getAiming = _ => arg.GetBoltState<IPlayerState>().mecanim.Aiming;
      PlayerCamera.instance.getHealth = _ => arg.GetBoltState<IPlayerState>().health;
      PlayerCamera.instance.getPitch = _ => arg.GetBoltState<IPlayerState>().pitch;

      // set camera target
      PlayerCamera.instance.SetTarget(arg);
    }
  }
}
