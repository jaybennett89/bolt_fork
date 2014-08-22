using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour {
  [SerializeField]
  Transform target;

  void OnTriggerEnter (Collider c) {
    BoltEntity be = c.GetComponent<BoltEntity>();

    if (be.boltIsOwner) {
      be.Teleport(target.position);
    }
  }
}
