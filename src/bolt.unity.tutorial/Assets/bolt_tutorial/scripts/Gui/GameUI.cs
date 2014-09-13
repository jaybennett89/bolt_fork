using UnityEngine;
using System.Collections;

public class GameUI : BoltSingletonPrefab<GameUI> {
  void Start() {
    if (!camera) {
      gameObject.AddComponent<Camera>();
    }

    camera.isOrthoGraphic = true;
    camera.cullingMask = 1 << LayerMask.NameToLayer("GUI");
    camera.nearClipPlane = 0;
    camera.farClipPlane = 500f;
    camera.useOcclusionCulling = false;
    camera.depth = 1;
    camera.clearFlags = CameraClearFlags.Depth;

    transform.position = new Vector3(0, 0, -250f);
  }

  void Update() {
    if (camera) {
      camera.orthographicSize = Screen.height / 2;
    }
  }
}
