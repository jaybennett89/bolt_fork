using UnityEngine;

/// <summary>
/// Interpolates the transform between two FixedUpdate states for smooth rendering without aliasing
/// </summary>
[BoltExecutionOrder(7500)]
public class BoltFrameInterpolator : MonoBehaviour {

  /// <summary>
  /// Progress to the next fixed update frame
  /// </summary>
  public static float renderAlpha {
    get { return Mathf.Clamp01((BoltCore.time - BoltCore.frameBeginTime) / BoltCore.frameDeltaTime); }
  }

  Vector3 _posPrevious;
  Vector3 _posCurrent;
  Vector3 _posRender;

  Quaternion _rotPrevious;
  Quaternion _rotCurrent;
  Quaternion _rotRender;

#pragma warning disable 0649
  [SerializeField]
  bool rotation = true;

  [SerializeField]
  bool position = true;

  [SerializeField]
  Transform model;

  [SerializeField]
  Vector3 offset;
#pragma warning restore 0649

  /// <summary>
  /// The current render position
  /// </summary>
  public Vector3 renderPosition {
    get { return _posRender; }
  }

  /// <summary>
  /// The current render rotation
  /// </summary>
  public Quaternion renderRotation {
    get { return _rotRender; }
  }

  /// <summary>
  /// The current render target
  /// </summary>
  public Transform renderTarget {
    get { return model; }
  }

  void Awake () {
    _posPrevious = _posCurrent = _posRender = transform.position;
    _rotPrevious = _rotCurrent = _rotRender = transform.rotation;

    if (!model) {
      BoltLog.Error("model not assigned to interpolator for '{0}'", gameObject.name);
    }
  }

  void LateUpdate () {
    if (model) {
      if (position) {
        model.position = _posRender = Vector3.Lerp(_posPrevious + offset, _posCurrent + offset, renderAlpha);
      }

      if (rotation) {
        model.rotation = _rotRender = Quaternion.Lerp(_rotPrevious, _rotCurrent, renderAlpha);
      }
    }
  }

  void FixedUpdate () {
    _posPrevious = _posCurrent;
    _posCurrent = transform.position;

    _rotPrevious = _rotCurrent;
    _rotCurrent = transform.rotation;
  }
}
