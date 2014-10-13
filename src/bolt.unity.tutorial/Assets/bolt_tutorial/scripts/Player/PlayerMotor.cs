using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PlayerMotor : MonoBehaviour {

  public struct State {
    public Vector3 position;
    public Vector3 velocity;
    public bool isGrounded;
    public int jumpFrames;
  }

  State _state;
  CharacterController _cc;

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

  Vector3 rayStart {
    get {
      Vector3 p;

      p = transform.position;
      p = p + _cc.center;

      return p;
    }
  }

  float rayDistance {
    get {
      return _cc.center.y + (skinWidth * 2f);
    }
  }

  public bool jumpStartedThisFrame {
    get {
      return _state.jumpFrames == (jumpTotalFrames - 1);
    }
  }

  void Awake() {
    _cc = GetComponent<CharacterController>();
    _state = new State();
    _state.position = transform.localPosition;
  }

  public void SetState(IPlayerCommandResult result) {

    // assign new state
    _state.position = result.position;
    _state.velocity = result.velocity;
    _state.jumpFrames = result.jumpFrames;
    _state.isGrounded = result.isGrounded;

    // assign local position
    transform.localPosition = _state.position;
  }

  void DrawRay(Ray r, float d, Color c) {
    Debug.DrawLine(r.origin, r.origin + (r.direction * d), c);
  }

  IEnumerable<RaycastHit> FindGround(Vector3 z, int count) {
    for (int i = 0; i < count; ++i) {
      Ray r;
      RaycastHit rh;

      r = new Ray();
      r.direction = Vector3.down;
      r.origin = rayStart + (Quaternion.Euler(0, i * (360 / count), 0) * z);

      if (Physics.Raycast(r, out rh, rayDistance, layerMask)) {
        DrawRay(r, rayDistance, Color.green);
        yield return rh;
      }
      else {
        DrawRay(r, rayDistance, Color.red);
      }
    }

    yield break;
  }

  public State Move(IPlayerCommandInput input) {

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

    // find ground (if any)
    var hits =
      FindGround(new Vector3(0, 0, _cc.radius), 8)
        .Concat(FindGround(new Vector3(0, 0, _cc.radius * 0.5f), 4))
        .Concat(FindGround(new Vector3(0, 0, 0), 1))
        .OrderBy(x => x.distance)
        .ToList();


    // if we had any ray hits, we are grounded
    if (hits.Count > 0) {
      // if we were not grounded before, zero out or velocity
      if (!_state.isGrounded) {
        _state.velocity.x = 0f;
        _state.velocity.y = 0f;
        _state.velocity.z = 0f;
      }

      if (_state.jumpFrames < (jumpTotalFrames / 2)) {
        Vector3 p;

        p = transform.position;
        p.y = (hits[0].point.y + skinWidth);

        transform.position = p;
      }

      _state.isGrounded = true;
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
}
