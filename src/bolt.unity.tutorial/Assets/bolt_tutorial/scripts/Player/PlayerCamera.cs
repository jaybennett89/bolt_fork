using UnityEngine;

public class PlayerCamera : BoltSingletonPrefab<PlayerCamera> {
  // damp velocity of camera
  Vector3 _velocity;

  // camera target
  Transform _target;

  // camera target state
  IPlayerState _targetState;

  // if we are aiming or not
  bool _aiming = false;

  // current camera distance
  float _distance = 0f;

  // accumulated time for aiming transition
  float _aimingAcc = 0f;

  [SerializeField]
  Transform cam;

  [SerializeField]
  float height = 2.3f;

  [SerializeField]
  float offset = 0.75f;

  [SerializeField]
  float aimingDistance = 1f;

  [SerializeField]
  float runningDistance = 3f;

  [SerializeField]
  float runningSmoothTime = 0.1f;

  [SerializeField]
  Transform dummyRig;

  [SerializeField]
  Transform dummyTarget;

  public new Camera camera {
    get { return cam.camera; }
  }

  void Awake() {
    _distance = runningDistance;
  }

  void LateUpdate() {
    UpdateCamera(true);
  }

  void UpdateCamera(bool allowSmoothing) {
    if (_target) {
      GrayscaleEffect ge = GetComponentInChildren<GrayscaleEffect>();

      if (_targetState.health >= 85) {
        ge.ramp = 0f;
      }
      else {
        ge.ramp = 1f - ((_targetState.health / 85f));
      }

      Screen.lockCursor = true;
      Screen.showCursor = false;

      if (_aiming) {
        if (_targetState.mecanim.Aiming == false) {
          _aiming = false;
          _aimingAcc = 0f;
        }
      }
      else {
        if (_targetState.mecanim.Aiming) {
          _aiming = true;
          _aimingAcc = 0f;
        }
      }

      _aimingAcc += Time.deltaTime;

      if (_aiming) {
        _distance = Mathf.Lerp(_distance, aimingDistance, _aimingAcc / 0.4f);
      }
      else {
        _distance = Mathf.Lerp(_distance, runningDistance, _aimingAcc / 0.4f);
      }

      Vector3 p;
      Quaternion r;

      CalculateCameraTransform(_target, _targetState, _distance, out p, out r);

      if (!_aiming || allowSmoothing) {
        p = Vector3.SmoothDamp(transform.position, p, ref _velocity, runningSmoothTime);
      }

      transform.position = p;
      transform.rotation = r;

      cam.transform.localRotation = Quaternion.identity;
      cam.transform.localPosition = Vector3.zero;
    }
  }

  public void SetTarget(BoltEntity entity) {
    _target = entity.transform;
    _targetState = entity.GetBoltState<IPlayerState>();

    UpdateCamera(false);
  }

  public void CalculateCameraAimTransform(Transform target, IPlayerState targetState, out Vector3 pos, out Quaternion rot) {
    CalculateCameraTransform(target, targetState, aimingDistance, out pos, out rot);
  }

  public void CalculateCameraTransform(Transform target, IPlayerState targetState, float distance, out Vector3 pos, out Quaternion rot) {

    // copy transform to dummy
    dummyTarget.position = target.position;
    dummyTarget.rotation = target.rotation;

    // move position to where we want it
    dummyTarget.position += new Vector3(0, height, 0);
    dummyTarget.position += dummyTarget.right * offset;

    // clamp and calculate pitch rotation
    Quaternion pitchRotation = Quaternion.Euler(targetState.pitch, 0, 0);

    pos = dummyTarget.position;
    pos += (-dummyTarget.forward * distance);

    pos = dummyTarget.InverseTransformPoint(pos);
    pos = pitchRotation * pos;
    pos = dummyTarget.TransformPoint(pos);

    // calculate look-rotation by setting position and looking at target
    dummyRig.position = pos;
    dummyRig.LookAt(dummyTarget.position);

    rot = dummyRig.rotation;
  }
}