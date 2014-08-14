using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarUserControl : MonoBehaviour {
  CarController car;

  void Awake () {
    car = GetComponent<CarController>();
  }

  void FixedUpdate () {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
    car.Move(h, v);
  }
}
