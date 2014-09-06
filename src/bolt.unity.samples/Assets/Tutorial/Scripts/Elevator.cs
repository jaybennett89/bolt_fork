using UnityEngine;
using System.Collections;

public class Elevator : MonoBehaviour {
  void OnTriggerEnter (Collider c) {
    BoltEntity entity = c.GetComponent<BoltEntity>();

    if (entity && entity.boltIsOwner) {
      entity.SetOrigin(transform);
    }
  }

  void OnTriggerExit (Collider c) {
    BoltEntity entity = c.GetComponent<BoltEntity>();

    if (entity && entity.boltIsOwner) {
      entity.SetOrigin(null);
    }
  }

  void FixedUpdate () {
    Vector3 p = transform.position;
    p.y = Mathf.PingPong(BoltNetwork.serverTime, 10f) - 1f;
    //p.x = Mathf.PingPong(BoltNetwork.serverTime, 20f);

    transform.position = p;
  }
}
