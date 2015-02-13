using UnityEngine;
using System.Collections;

public class Sphere : Bolt.EntityBehaviour<ISphereState> {
  [SerializeField]
  Transform renderTransform;

  public override void Attached() {
    state.Transform.SetTransforms(transform, renderTransform);
  }
}
