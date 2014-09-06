using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

public class TeleportPlayerController : BoltEntityBehaviour<TeleportPlayerSerializer, ITeleportPlayerState> {
  bool left;
  bool right;
  bool forward;
  bool backward;

  float yRotation = 0f;
  float mouseSensitivity = 2f;

  TeleportPlayerMotor motor;

  void Awake () {
    motor = GetComponent<TeleportPlayerMotor>();
  }

  void PollKeys () {
    left = Input.GetKey(KeyCode.A);
    right = Input.GetKey(KeyCode.D);
    forward = Input.GetKey(KeyCode.W);
    backward = Input.GetKey(KeyCode.S);
  }

  void ClearKeys () {
    left = false;
    right = false;
    forward = false;
    backward = false;
  }

  void Update () {
    if (boltEntity.boltIsControlling) {
      PollKeys();

      if (Input.GetButton("Fire2")) {
        yRotation += (Input.GetAxisRaw("Mouse X") * mouseSensitivity);
      }

      TeleportCamera.instance.rotateCameraBehindTarget = left || right || forward || backward;
    }
  }

  public override void SimulateController () {
    PollKeys();

    TeleportPlayerCommand playerCmd = BoltFactory.NewCommand<TeleportPlayerCommand>();
    playerCmd.input.left = left;
    playerCmd.input.right = right;
    playerCmd.input.forward = forward;
    playerCmd.input.backward = backward;
    playerCmd.input.yRotation = yRotation;
    playerCmd.input.shoot = Input.GetMouseButton(0);

    boltEntity.QueueCommand(playerCmd);

    ClearKeys();
  }

  public override void ExecuteCommand (BoltCommand cmd, bool resetState) {
    TeleportPlayerCommand playerCmd = (TeleportPlayerCommand) cmd;

    if (resetState) {

      motor.SetState(playerCmd.state.position, playerCmd.state.velocity, playerCmd.state.grounded);

    } else {

      Vector3 movement = Vector3.zero;

      if (playerCmd.input.left) { movement.x -= 1; }
      if (playerCmd.input.right) { movement.x += 1; }
      if (playerCmd.input.forward) { movement.z += 1; }
      if (playerCmd.input.backward) { movement.z -= 1; }

      motor.transform.rotation = Quaternion.Euler(0, playerCmd.input.yRotation, 0);
      motor.Move(motor.transform.rotation * movement);

      playerCmd.state.position = motor.position;
      playerCmd.state.velocity = motor.velocity;
      playerCmd.state.grounded = motor.grounded;

      if (playerCmd.isFirstExecution) {
        if (playerCmd.input.shoot) {
          GetComponentInChildren<Animator>().SetLayerWeightReflected(1, 1f);
        } else {
          GetComponentInChildren<Animator>().SetLayerWeightReflected(1, 0f);
        }

        float x = 0;
        float z = 0;

        if (playerCmd.input.forward) { z = +1; }
        if (playerCmd.input.backward) { z = -1; }

        if (playerCmd.input.left) { x = -1; }
        if (playerCmd.input.right) { x = +1; }

        GetComponentInChildren<Animator>().SetFloatReflected("MoveX", x);
        GetComponentInChildren<Animator>().SetFloatReflected("MoveZ", z);
      }
    }
  }
}
