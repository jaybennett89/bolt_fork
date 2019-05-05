using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt {
  [Documentation]
  public enum HitboxShape {
    Box,
    Sphere
  }

  [Serializable]
  public class Hitbox {
    public Transform Bone = null;
    public HitboxShape Shape = HitboxShape.Box;
    public Vector3 Center = Vector3.zero;
    public Vector3 Size = new Vector3(0.25f, 0.25f, 0.25f);
    public float Radius = 0.25f;
  }

  public class HitboxBody : MonoBehaviour {
    [SerializeField]
    internal int HitboxCount;

    [SerializeField]
    internal Hitbox[] HitboxArray = new Hitbox[HitboxBodySnapshot.MAX_HITBOXES];
  }
}
