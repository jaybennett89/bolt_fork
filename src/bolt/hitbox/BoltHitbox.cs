using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Defines one hitbox on a BoltHitboxBody
/// </summary>
/// <example>
/// *Example:* Resizing a sphere hitbox
/// 
/// ```csharp
/// void ResizeSphereHitbox(BoltHitbox hitbox, float newRadius) {
///   if(hitbox.hitboxShape != BoltHitboxShape.Sphere) {
///     Debug.Log("Attemping to resize a non-sphere hitbox");
///     return;
///   }
///   
///   hitbox.hitboxSphereRadius = newRadius;
/// }
/// ```
/// </example>
[Documentation]
public class BoltHitbox : MonoBehaviour {

  [SerializeField]
  internal BoltHitboxShape _shape = BoltHitboxShape.Box;

  [SerializeField]
  internal BoltHitboxType _type = BoltHitboxType.Unknown;

  [SerializeField]
  internal Vector3 _center = Vector3.zero;

  [SerializeField]
  internal Vector3 _boxSize = new Vector3(0.25f, 0.25f, 0.25f);

  [SerializeField]
  internal float _sphereRadius = 0.25f;

  /// <summary>
  /// Shape of the hitbox (box or sphere)
  /// </summary>
  /// <example>
  /// *Example:* Sorting the hitboxes in a body based on shape.
  /// 
  /// ```csharp
  /// void ConfigureHitboxes(BoltHitboxBody body) {
  ///   foreach(BoltHitbox hitbox in body.hitboxes) {
  ///     switch(hitbox.hitboxShape) {
  ///       case BoltHitboxShape.Sphere: ConfigureSphere(hitbox); break;
  ///       case BoltHitboxShape.Box: ConfigureBox(hitbox); break;
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public BoltHitboxShape hitboxShape {
    get { return _shape; }
    set { _shape = value; }
  }

  /// <summary>
  /// Type of the hitbox
  /// </summary>
  /// <example>
  /// *Example:* Modifying a base damage value depending on the area of the hit.
  /// 
  /// ```csharp
  /// float CalculateDamage(BoltHitbox hit, float baseDamage) {
  ///   switch(hit.hitboxType) {
  ///     case BoltHitboxType.Head: return 2.0f * baseDamage;
  ///     
  ///     case BoltHitboxType.Leg:
  ///     case BoltHitboxType.UpperArm: return 0.7f * baseDamage;
  ///     
  ///     default: return baseDamage;
  ///   }
  /// }
  /// ```
  /// </example>
  public BoltHitboxType hitboxType {
    get { return _type; }
    set { _type = value; }
  }

  /// <summary>
  /// Center of the hitbox in local coordinates
  /// </summary>
  /// <example>
  /// *Example:* Getting a vector that points from the player's weapon to the head of a target entity.
  /// 
  /// ```csharp
  /// Vector3 GetHeadshotVector(BoltEntity target, IWeapon currentWeapon) {
  ///   BoltHitboxBody body = target.GetComponent&ltBoltHitboxBody%gt();
  ///   BoltHitbox head = body.hitboxes[0];
  ///   
  ///   foreach(BoltHitbox hitbox in body.hitboxes) {
  ///     if(hitbox.hitboxType == BoltHitboxType.Head) {
  ///       head = hitbox;
  ///     }
  ///   }
  ///   
  ///   return head.hitboxCenter - currentWeapon.fireOrigin;
  /// }
  /// ```
  /// </example>
  public Vector3 hitboxCenter {
    get { return _center; }
    set { _center = value; }
  }

  /// <summary>
  /// Size of the hitbox if this shape is a box
  /// </summary>
  /// <example>
  /// *Example:* A method to double the size of a player's head hitbox if it is a box.
  /// 
  /// ```csharp
  /// void DoubleHeadSize(BoltHitboxBody body) {
  ///   foreach(BoltHitbox hitbox in body.hitboxes) {
  ///     if(hitbox.hitboxType == BoltHitboxType.Head) {
  ///       hitbox.hitboxBoxSize = hitbox.hitboxBoxSize * 2f;
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public Vector3 hitboxBoxSize {
    get { return _boxSize; }
    set { _boxSize = value; }
  }

  /// <summary>
  /// Type of the hitbox
  /// </summary>
  /// <example>
  /// *Example:* A method to double the size of a player's head hitbox if it is a sphere.
  /// 
  /// ```csharp
  /// void DoubleHeadSize(BoltHitboxBody body) {
  ///   foreach(BoltHitbox hitbox in body.hitboxes) {
  ///     if(hitbox.hitboxType == BoltHitboxType.Head) {
  ///       hitbox.hitboxSphereRadius = hitbox.hitboxSphereRadius * 2f;
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public float hitboxSphereRadius {
    get { return _sphereRadius; }
    set { _sphereRadius = value; }
  }

  void OnDrawGizmos () {
    Draw(transform.localToWorldMatrix);
  }

  internal void Draw (Matrix4x4 matrix) {
    Gizmos.color = new Color(255f / 255f, 128f / 255f, 39f / 255f);
    Gizmos.matrix = matrix;

    switch (_shape) {
      case BoltHitboxShape.Box:
        Gizmos.DrawWireCube(_center, _boxSize);
        break;

      case BoltHitboxShape.Sphere:
        Gizmos.DrawWireSphere(_center, _sphereRadius);
        break;
    }

    Gizmos.matrix = Matrix4x4.identity;
    Gizmos.color = Color.white;
  }

  internal bool OverlapSphere (ref Matrix4x4 matrix, Vector3 center, float radius) {
    center = matrix.MultiplyPoint(center);

    switch (_shape) {
      case BoltHitboxShape.Box:
        return OverlapSphereOnBox(center, radius);

      case BoltHitboxShape.Sphere:
        return OverlapSphereOnSphere(center, radius);

      default:
        return false;
    }
  }

  internal bool Raycast (ref Matrix4x4 matrix, Vector3 origin, Vector3 direction, out float distance) {
    origin = matrix.MultiplyPoint(origin);
    direction = matrix.MultiplyVector(direction);

    switch (_shape) {
      case BoltHitboxShape.Box:
        Bounds b = new Bounds(_center, _boxSize);
        return b.IntersectRay(new Ray(origin, direction), out distance);

      case BoltHitboxShape.Sphere:
        return RaycastSphere(origin, direction, out distance);

      default:
        distance = 0f;
        return false;
    }
  }

  bool OverlapSphereOnSphere (Vector3 center, float radius) {
    return Vector3.Distance(_center, center) <= _sphereRadius + radius;
  }

  bool OverlapSphereOnBox (Vector3 center, float radius) {
    Bounds b = new Bounds(_center, _boxSize);

    Vector3 clampedCenter;
    Vector3 min = b.min;
    Vector3 max = b.max;

    ClampVector(ref center, ref min, ref max, out clampedCenter);

    return Vector3.Distance(center, clampedCenter) <= radius;
  }

  bool RaycastSphere (Vector3 o, Vector3 d, out float distance) {
    Vector3 v = o - _center;
    float b = Vector3.Dot(v, d);
    float c = Vector3.Dot(v, v) - (_sphereRadius * _sphereRadius);

    if (c > 0f && b > 0f) {
      distance = 0f;
      return false;
    }

    float disc = b * b - c;

    if (disc < 0f) {
      distance = 0f;
      return false;
    }

    distance = -b - (float) System.Math.Sqrt(disc);

    if (distance < 0f) {
      distance = 0f;
    }

    return true;
  }

  static void ClampVector (ref Vector3 value, ref Vector3 min, ref Vector3 max, out Vector3 result) {
    float x = value.x;
    x = (x > max.x) ? max.x : x;
    x = (x < min.x) ? min.x : x;

    float y = value.y;
    y = (y > max.y) ? max.y : y;
    y = (y < min.y) ? min.y : y;

    float z = value.z;
    z = (z > max.z) ? max.z : z;
    z = (z < min.z) ? min.z : z;

    result = new Vector3(x, y, z);
  }
}
