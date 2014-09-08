using UnityEngine;

public abstract class BoltStateTransform {
  protected BoltEntity _entity;
  protected IBoltState _state;
  protected Transform _transform;

  protected bool _stepOnProxy = true;
  protected bool _stepOnController = false;

  protected bool shouldStep {
    get {
      if (_entity.isOwner) {
        return false;
      } else if (_entity.hasControl) {
        return _stepOnController;
      } else {
        return _stepOnProxy;
      }
    }
  }

  protected Vector3 transformPosition {
    get { return _transform.localPosition; }
    set { _transform.localPosition = value; }
  }

  protected Quaternion transformRotation {
    get { return _transform.localRotation; }
    set { _transform.localRotation = value; }
  }

  protected BoltStateTransform (BoltEntity entity, IBoltState state) {
    _entity = entity;
    _state = state;
    _transform = entity.transform;
  }
}
