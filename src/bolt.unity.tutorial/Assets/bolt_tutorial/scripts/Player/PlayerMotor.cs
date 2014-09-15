﻿using UnityEngine;
using System.Linq;
using System.Collections;

public class PlayerMotor : MonoBehaviour {

  CharacterController _cc;
  PlayerCommand.State _state;

  [SerializeField]
  float skinWidth = 0.08f;

  [SerializeField]
  float gravityForce = -9.81f;

  [SerializeField]
  float jumpForce = +40f;

  [SerializeField]
  int jumpTotalFrames = 30;

  [SerializeField]
  float movingSpeed = 4f;

  [SerializeField]
  float maxVelocity = 32f;

  [SerializeField]
  Vector3 drag = new Vector3(1f, 0f, 1f);

  [SerializeField]
  LayerMask layerMask;

  float sphereRadius {
    get { return _cc.radius; }
  }

  Vector3 feetPosition {
    get {
      Vector3 p = transform.position;

      p.y += _cc.radius;
      p.y += _cc.center.y;
      p.y -= (_cc.height * 0.5f);
      p.y -= (skinWidth * 1.25f);

      return p;
    }
  }

  Vector3 feetLineCheck {
    get {
      return
        feetPosition + (new Vector3(0, -sphereRadius * 1.35f, 0));
    }
  }

  public bool jumpStartedThisFrame {
    get {
      return _state.jumpFrames == (jumpTotalFrames - 1);
    }
  }

  void Awake() {
    _cc = GetComponent<CharacterController>();
    _state = new PlayerCommand.State();
    _state.position = transform.localPosition;
  }

  public void SetState(PlayerCommand.State state) {
    // assign new state
    _state = state;

    // assign local position
    transform.localPosition = _state.position;
  }

  public PlayerCommand.State Move(PlayerCommand.Input input) {
    var moving = false;
    var movingDir = Vector3.zero;

    if (input.forward ^ input.backward) {
      movingDir.z = input.forward ? +1 : -1;
    }

    if (input.left ^ input.right) {
      movingDir.x = input.right ? +1 : -1;
    }

    if (movingDir.x != 0 || movingDir.z != 0) {
      moving = true;
      movingDir = Vector3.Normalize(Quaternion.Euler(0, input.yaw, 0) * movingDir);
    }

    // clamp us to the ground
    RaycastHit hit;

    if (Physics.Linecast(feetPosition, feetLineCheck, out hit, layerMask)) {
      if (_state.jumpFrames < (jumpTotalFrames / 2)) {
        Vector3 p;

        p = hit.point;
        p.y += skinWidth;

        transform.position = p;
      }
    }

    // update grounded state
    var overlapping = Physics.OverlapSphere(feetPosition, sphereRadius, layerMask);
    if (overlapping.Count(x => x.isTrigger == false && (x is CharacterController) == false) > 0) {
      if (_state.isGrounded == false) {
        _state.isGrounded = true;

        _state.velocity.x = 0f;
        _state.velocity.y = 0f;
        _state.velocity.z = 0f;
      }
    }
    else {
      _state.isGrounded = false;
    }

    //
    if (_state.isGrounded) {
      if (input.jump && _state.jumpFrames == 0) {
        _state.jumpFrames = (byte)jumpTotalFrames;
        _state.velocity += movingDir * movingSpeed;
      }

      if (moving) {
        _cc.Move(movingDir * movingSpeed * BoltNetwork.frameDeltaTime);
        _state.position = transform.localPosition;
      }
    }
    else {
      _state.velocity.y += gravityForce * BoltNetwork.frameDeltaTime;
    }

    if (_state.jumpFrames > 0) {
      // calculate force
      float force;

      force = (float)_state.jumpFrames / (float)jumpTotalFrames;
      force = jumpForce * force;

      _cc.Move(new Vector3(0, force * BoltNetwork.frameDeltaTime, 0));
    }

    // clamp velocity
    _state.velocity.x = Mathf.Clamp(_state.velocity.x, -maxVelocity, +maxVelocity);
    _state.velocity.y = Mathf.Clamp(_state.velocity.y, -maxVelocity, +maxVelocity);
    _state.velocity.z = Mathf.Clamp(_state.velocity.z, -maxVelocity, +maxVelocity);

    // apply drag
    _state.velocity.x = ApplyDrag(_state.velocity.x, drag.x);
    _state.velocity.y = ApplyDrag(_state.velocity.y, drag.y);
    _state.velocity.z = ApplyDrag(_state.velocity.z, drag.z);

    // decrease jump frames
    _state.jumpFrames = (byte)Mathf.Max(0, _state.jumpFrames - 1);

    // apply velocity
    _cc.Move(_state.velocity * BoltNetwork.frameDeltaTime);

    // set local rotation
    transform.localRotation = Quaternion.Euler(0, input.yaw, 0);

    // update position
    _state.position = transform.localPosition;

    // done
    return _state;
  }

  float ApplyDrag(float value, float drag) {
    if (value < 0) {
      return Mathf.Min(value + (drag * BoltNetwork.frameDeltaTime), 0);
    }

    else if (value > 0) {
      return Mathf.Max(value - (drag * BoltNetwork.frameDeltaTime), 0);
    }

    return value;
  }

  void OnDrawGizmos() {
    if (Application.isPlaying && _cc) {
      Gizmos.color = _state.isGrounded ? Color.green : Color.red;
      Gizmos.DrawWireSphere(feetPosition, sphereRadius);

      Gizmos.color = Color.magenta;
      Gizmos.DrawLine(feetPosition, feetLineCheck);

      //Gizmos.color = Color.white;
      //Gizmos.DrawSphere(fp, 0.1f);
    }
  }
}