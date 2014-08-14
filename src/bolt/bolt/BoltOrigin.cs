using System.Collections.Generic;
using UnityEngine;

public sealed class BoltOrigin : MonoBehaviour {
  static readonly Dictionary<int, BoltOrigin> transforms = new Dictionary<int, BoltOrigin>();

  [SerializeField]
  internal int _transformId = 0;

  void OnEnable () {
    transforms.Add(_transformId, this);
  }

  void OnDisable () {
    transforms.Remove(_transformId);
  }

  public static Transform ResolveTransform (int id) {
    return transforms[id].transform;
  }

  public static int ResolveTransformId (Transform transform) {
    return transform.GetComponent<BoltOrigin>()._transformId;
  }
}
