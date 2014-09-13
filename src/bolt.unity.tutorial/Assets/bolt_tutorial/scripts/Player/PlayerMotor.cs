using UnityEngine;
using System.Linq;
using System.Collections;

public class PlayerMotor : MonoBehaviour {
  const byte TOTAL_JUMP_FRAMES = 60;

  CharacterController _cc;
  PlayerCommand.State _state;

  [SerializeField]
  float skinWidth = 0.08f;

  [SerializeField]
  float gravityForce = -9.81f;

  [SerializeField]
  float jumpForce = +20f;

  [SerializeField]
  float movingSpeed = 4f;

  [SerializeField]
  float maxVelocity = 32f;

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
      p.y -= (skinWidth * 1.1f);

      return p;
    }
  }

  Vector3 feetLineCheck {
    get {
      return 
        feetPosition + (new Vector3(0, -sphereRadius * 1.1f, 0));  
    }
  }

  void Awake() {
    _cc = GetComponent<CharacterController>();
    _state = new PlayerCommand.State();
    _state.position = transform.localPosition;
  }

  public void SetState(PlayerCommand.State state) {
    _state = state;
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
      Vector3 p;

      p = hit.point;
      p.y += skinWidth;

      transform.position = p;
    }

    // update grounded state
    var overlapping = Physics.OverlapSphere(feetPosition, sphereRadius, layerMask);


    Debug.Log(string.Join(", ", overlapping.Select(x => x.gameObject.name).ToArray()));

    if (overlapping.Count(x => x.isTrigger == false && (x is CharacterController) == false) > 0) {
      if (_state.isGrounded == false) {
        _state.isGrounded = true;
        _state.velocity.y = 0f;
      }
    }
    else {
      _state.isGrounded = false;
    }

    //
    if (_state.isGrounded) {
      //if (input.jump && _state.jumpFrames == 0) {
      //_state.jumpFrames = TOTAL_JUMP_FRAMES;
      //}

      if (moving) {
        _cc.Move(movingDir * movingSpeed * BoltNetwork.frameDeltaTime);
        _state.position = transform.localPosition;
      }
    }
    else {
      _state.velocity.y += gravityForce * BoltNetwork.frameDeltaTime;
    }

    _state.velocity.y = Mathf.Clamp(_state.velocity.y, -maxVelocity, +maxVelocity);

    // apply velocity
    _cc.Move(_state.velocity * BoltNetwork.frameDeltaTime);

    // set local rotation
    transform.localRotation = Quaternion.Euler(0, input.yaw, 0);

    // done
    return _state;
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
