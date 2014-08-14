using UnityEngine;

public class TutorialCallbacks : BoltCallbacks {
  public override void ControlOfEntityGained (BoltEntity arg) {
    TutorialPlayerCamera.instance.target = arg.transform;
  }

  public override void OnEvent (IToggleRain evnt, BoltConnection cn) {
    // find the rain game object
    GameObject rain = TutorialPlayerCamera.instance.transform.FindChild("Rain").gameObject;

    // invert the active status, so true=>false and false=>true
    rain.SetActive(!rain.activeSelf);
  }
}
