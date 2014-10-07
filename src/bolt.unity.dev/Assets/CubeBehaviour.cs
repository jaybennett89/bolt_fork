using UnityEngine;
using System.Collections;

public class CubeBehaviour : Bolt.EntityBehaviour<IPlayer> {
  [SerializeField]
  Transform render;

  public override void Attached() {
    if (entity.hasControl) {
      render.renderer.material.color = Color.green;
    }

    state.SetAnimator(GetComponent<Animator>());
    state.Transform.SetTransforms(transform, render);
  }

  public override void SimulateController() {
    IPlayerCommandInput input = PlayerCommand.Create();
    input.Forward = true;

    entity.QueueInput(input);
  }

  public override void ExecuteCommand(Bolt.Command command, bool resetState) {
    PlayerCommand cmd = (PlayerCommand)command;

    if (resetState) {
      transform.position = cmd.Result.Position;
    }
    else {
      Vector3 move = new Vector3(10f * BoltNetwork.frameDeltaTime, 0, 0);
      transform.Translate(move, Space.Self);

      // save position
      cmd.Result.Position = transform.position;
    }
  }

  public override void ControlLost() {
    render.renderer.material.color = Color.red;
  }
}
