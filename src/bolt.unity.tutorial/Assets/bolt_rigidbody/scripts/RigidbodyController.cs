using UnityEngine;
using System.Collections;

public class RigidbodyController : Bolt.EntityBehaviour<IRigidbodyState> {
  public override void Attached() {
    state.Transform.SetTransforms(transform);
  }

  public override void SimulateController() {
    
  }

  Vector3 position;
  Quaternion rotation;

  Vector3 velocity;
  Vector3 velocityAngular;

  public override void ExecuteCommand(Bolt.Command command, bool resetState) {
    RigidbodyCommand cmd = (RigidbodyCommand)command;

    if (resetState) {
      position = cmd.Result.Position;
      rotation = cmd.Result.Rotation;
      velocity = cmd.Result.Velocity;
      velocityAngular = cmd.Result.VelocityAngular;
    }
    else {
      // this command is being re-applied
      if (cmd.IsFirstExecution == false) {

      }
      else {

      }
    }
  }
}
