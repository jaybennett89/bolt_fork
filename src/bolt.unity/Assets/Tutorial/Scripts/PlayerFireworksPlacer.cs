using UnityEngine;
using System.Collections;

public class PlayerFireworksPlacer : BoltEntityBehaviour {
  void Update () {
    if (Input.GetKeyDown(KeyCode.Q)) {
      IPlaceFirework evnt = BoltFactory.NewEvent<IPlaceFirework>();
      evnt.position = new Vector3(transform.position.x, 0, transform.position.z);
      boltEntity.Raise(evnt);
    }
  }

  public override void OnEvent (IPlaceFirework evnt, BoltConnection cn) {
    GameObject prefab = (GameObject) Resources.Load("Fireworks", typeof(GameObject));
    GameObject.Instantiate(prefab, evnt.position, Quaternion.Euler(-90, 0, 0));
  }
}
