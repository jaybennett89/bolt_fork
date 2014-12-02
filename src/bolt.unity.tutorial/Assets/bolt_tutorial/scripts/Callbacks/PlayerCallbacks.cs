using UnityEngine;
using System.Collections;
using System.Text;

[BoltGlobalBehaviour("Level1")]
public class PlayerCallbacks : Bolt.GlobalEventListener {
  public override void SceneLoadLocalDone(string map) {
    // ui
    GameUI.Instantiate();

    // camera
    PlayerCamera.Instantiate();
  }

  public override void ControlOfEntityGained(BoltEntity arg, Bolt.IProtocolToken token) {
    BoltLog.Info("ControlGained-Token: {0}", token);

    // add audio listener to our character
    arg.gameObject.AddComponent<AudioListener>();

    // set camera callbacks
    PlayerCamera.instance.getAiming = () => arg.GetState<IPlayerState>().Aiming;
    PlayerCamera.instance.getHealth = () => arg.GetState<IPlayerState>().health;
    PlayerCamera.instance.getPitch = () => arg.GetState<IPlayerState>().pitch;

    // set camera target
    PlayerCamera.instance.SetTarget(arg);
  }
}
