using Bolt;
using System;
using System.Collections.Generic;

/// <summary>
/// Describes a hit to a BoltHitbox on a BoltHitboxBody
/// </summary>
[Documentation]
public struct BoltPhysicsHit {
  /// <summary>
  /// The distance away from the origin of the ray
  /// </summary>
  public float distance;

  /// <summary>
  /// Which hitbox was hit
  /// </summary>
  public BoltHitbox hitbox;

  /// <summary>
  /// The body which was hit
  /// </summary>
  public BoltHitboxBody body;
}

/// <summary>
/// Container for a group of BoltPhysicsHits
/// </summary>
[Documentation]
public class BoltPhysicsHits : BoltObject, IDisposable {
  internal static readonly BoltObjectPool<BoltPhysicsHits> _pool = new BoltObjectPool<BoltPhysicsHits>();
  internal List<BoltPhysicsHit> _hits = new List<BoltPhysicsHit>();

  /// <summary>
  /// How many hits we have in the collection
  /// </summary>
  public int count {
    get { return _hits.Count; }
  }

  public BoltPhysicsHit this[int index] {
    get { return _hits[index]; }
  }

  /// <summary>
  /// Get the hit at a specific index
  /// </summary>
  public BoltPhysicsHit GetHit (int index) {
    return _hits[index];
  }

  public void Dispose () {
    _hits.Clear();
    _pool.Release(this);
  }

  internal void AddHit (BoltHitboxBody body, BoltHitbox hitbox, float distance) {
    _hits.Add(new BoltPhysicsHit { body = body, hitbox = hitbox, distance = distance });
  }
}
