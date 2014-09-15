using UnityEngine;

/// <summary>
/// Contains functions for raycasting against BoltHitboxBody objects
/// </summary>
public static class BoltPhysics {
  static int maxWorldSnapshots = 60;
  static readonly BoltDoubleList<BoltHitboxBody> _hitboxBodies = new BoltDoubleList<BoltHitboxBody>();
  static readonly BoltDoubleList<BoltHitboxWorldSnapshot> _worldSnapshots = new BoltDoubleList<BoltHitboxWorldSnapshot>();

  internal static void RegisterBody(BoltHitboxBody body) {
    _hitboxBodies.AddLast(body);
  }

  internal static void UnregisterBody(BoltHitboxBody body) {
    _hitboxBodies.Remove(body);
  }

  internal static void SnapshotWorld() {
    var it = _hitboxBodies.GetIterator();
    var sn = BoltHitboxWorldSnapshot._pool.Acquire();

    // set frame
    sn._frame = BoltCore.frame;

    // create snapshot
    while (it.Next()) {
      sn.Snapshot(it.val);
    }

    _worldSnapshots.AddLast(sn);

    while (_worldSnapshots.count > maxWorldSnapshots) {
      _worldSnapshots.RemoveFirst().Dispose();
    }
  }

  internal static void DrawSnapshot() {
#if DEBUG
    if (_worldSnapshots.count > 0) {
      _worldSnapshots.first.Draw();
    }
#endif
  }

  /// <summary>
  /// Cast a ray against the last frame
  /// </summary>
  public static BoltPhysicsHits Raycast(Ray ray) {
    if (_worldSnapshots.count > 0) {
      return Raycast(ray, _worldSnapshots.last);
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  /// <summary>
  /// Cast a ray against a specific frame number
  /// </summary>
  public static BoltPhysicsHits Raycast(Ray ray, int frame) {
    var it = _worldSnapshots.GetIterator();

    while (it.Next()) {
      if (it.val._frame == frame) {
        return Raycast(ray, it.val);
      }
    }

    if (_worldSnapshots.count > 0) {
      return Raycast(ray, _worldSnapshots.last);
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  /// <summary>
  /// Overlap a sphere against the last frame
  /// </summary>
  public static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius) {
    if (_worldSnapshots.count > 0) {
      return OverlapSphere(origin, radius, _worldSnapshots.last);
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  /// <summary>
  /// Overlap a sphere gainst a specific frame
  /// </summary>
  public static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius, int frame) {
    var it = _worldSnapshots.GetIterator();

    while (it.Next()) {
      if (it.val._frame == frame) {
        return OverlapSphere(origin, radius, it.val);
      }
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  static BoltPhysicsHits Raycast(Ray ray, BoltHitboxWorldSnapshot sn) {
    var it = sn._bodySnapshots.GetIterator();
    var hits = BoltPhysicsHits._pool.Acquire();

    while (it.Next()) {
      it.val.Raycast(ray.origin, ray.direction, hits);
    }

    return hits;
  }

  static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius, BoltHitboxWorldSnapshot sn) {
    var it = sn._bodySnapshots.GetIterator();
    var hits = BoltPhysicsHits._pool.Acquire();

    while (it.Next()) {
      it.val.OverlapSphere(origin, radius, hits);
    }

    return hits;
  }
}
