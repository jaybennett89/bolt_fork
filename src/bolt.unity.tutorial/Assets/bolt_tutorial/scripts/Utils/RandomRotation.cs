using UnityEngine;
using System.Collections;

public class RandomRotation : MonoBehaviour {
  [SerializeField]
  Vector3 axes = new Vector3(0, 0, 1);

  void OnEnable() {
    transform.localRotation = Quaternion.Euler(
      new Vector3(
        Random.Range(0, 360f) * axes.x,
        Random.Range(0, 360f) * axes.y,
        Random.Range(0, 360f) * axes.z
      )
    );
  }
}
