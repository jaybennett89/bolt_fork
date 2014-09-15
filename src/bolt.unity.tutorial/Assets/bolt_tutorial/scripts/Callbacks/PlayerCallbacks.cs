using UnityEngine;
using System.Collections;

//[BoltGlobalBehaviour]
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

      // set camera target
      PlayerCamera.instance.SetTarget(arg);
    }
  }
}
