using System;
using UnityEngine;

internal static class BoltPhysics {
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

  internal static Vector3 PositionAtFrame(BoltHitboxBody hitbox, int frame) {
      var it = _worldSnapshots.GetIterator();

      while (it.Next())
      {
          if (it.val._frame == frame)
          {
              return PositionAtFrame(hitbox, it.val);
          }
      }

      if (_worldSnapshots.count > 0)
      {
          return PositionAtFrame(hitbox, _worldSnapshots.last);
      }

      return hitbox._proximity._center;
  }

  internal static BoltPhysicsHits Raycast(Ray ray) {
    if (_worldSnapshots.count > 0) {
      return Raycast(ray, _worldSnapshots.last);
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  internal static BoltPhysicsHits Raycast(Ray ray, int frame) {
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

  internal static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius) {
    if (_worldSnapshots.count > 0) {
      return OverlapSphere(origin, radius, _worldSnapshots.last);
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  internal static BoltPhysicsHits OverlapSphere(Vector3 origin, float radius, int frame) {
    var it = _worldSnapshots.GetIterator();

    while (it.Next()) {
      if (it.val._frame == frame) {
        return OverlapSphere(origin, radius, it.val);
      }
    }

    return BoltPhysicsHits._pool.Acquire();
  }

  static Vector3 PositionAtFrame(BoltHitboxBody hitbox, BoltHitboxWorldSnapshot sn) {
    var it = sn._bodySnapshots.GetIterator();

    while (it.Next()) {
      if (it.val.HitboxBody == hitbox) {
          return it.val.GetPosition();
      }
    }

    return hitbox._proximity._center;
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
