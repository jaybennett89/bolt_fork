using UnityEngine;

public interface IBoltEntityCallbacks {
  void BeforeStep ();
  void AfterStep ();
  void UpdateRender ();
  void Teleported ();
  void OriginChanging (Transform @old, Transform @new);
}
