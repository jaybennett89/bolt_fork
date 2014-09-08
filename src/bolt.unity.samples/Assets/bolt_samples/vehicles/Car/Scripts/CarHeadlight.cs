using UnityEngine;
using System.Collections;

public class CarHeadlight : BoltEntityBehaviour<CarSerializer, ICarState> {
  void Update () {
    GetComponent<Light>().enabled = boltState.headlights;
  }
}
