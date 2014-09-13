using UnityEngine;
using System.Collections;

public class PlayerController : BoltEntityBehaviour<IPlayerState> {
  const float MOUSE_SENSEITIVITY = 2f;

  PlayerCommand.Input _input;
  PlayerMotor _motor;

  void Awake() {
    _motor = GetComponent<PlayerMotor>();
  }

  void Update() {
    PollKeys(true);
  }

  void PollKeys(bool mouse) {
    _input.forward = Input.GetKey(KeyCode.W);
    _input.backward = Input.GetKey(KeyCode.S);
    _input.left = Input.GetKey(KeyCode.A);
    _input.right = Input.GetKey(KeyCode.D);
    _input.jump = Input.GetKey(KeyCode.Space);

    if (mouse) {
      _input.yaw += (Input.GetAxisRaw("Mouse X") * MOUSE_SENSEITIVITY);
      _input.yaw %= 360f;

      _input.pitch += (Input.GetAxisRaw("Mouse Y") * MOUSE_SENSEITIVITY);
      _input.pitch = Mathf.Clamp(_input.pitch, -85f, +85f);
    }
  }

  public override void SimulateController() {
    PollKeys(false);

    PlayerCommand cmd;

    cmd = BoltFactory.NewCommand<PlayerCommand>();
    cmd.input = this._input;

    entity.QueueCommand(cmd);
  }

  public override void ExecuteCommand(BoltCommand c, bool resetState) {
    PlayerCommand cmd = (PlayerCommand)c;

    if (resetState) {
      _motor.SetState(cmd.state);
    }
    else {
      // move and save the resulting state
      cmd.state = _motor.Move(cmd.input);

      if (cmd.isFirstExecution) {
        if (cmd.input.forward ^ cmd.input.backward) {
          state.mecanim.MoveZ = cmd.input.forward ? 1 : -1;
        }
        else {
          state.mecanim.MoveZ = 0;
        }

        if (cmd.input.left ^ cmd.input.right) {
          state.mecanim.MoveX = cmd.input.right ? 1 : -1;
        }
        else {
          state.mecanim.MoveX = 0;
        }

        if (_motor.jumpStartedThisFrame) {
          state.mecanim.Jump();
        }
      }
    }
  }
}
