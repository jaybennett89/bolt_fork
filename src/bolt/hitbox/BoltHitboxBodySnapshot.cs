using System;
using UnityEngine;

class BoltHitboxBodySnapshot : BoltObject, IDisposable {
  static readonly BoltObjectPool<BoltHitboxBodySnapshot> _pool = new BoltObjectPool<BoltHitboxBodySnapshot>();

  int _count = 0;
  BoltHitboxBody _body = null;
  Matrix4x4 _wtl = Matrix4x4.identity;
  Matrix4x4 _ltw = Matrix4x4.identity;
  Matrix4x4[] _hbwtl = new Matrix4x4[32];
  Matrix4x4[] _hbltw = new Matrix4x4[32];

  public void Snapshot (BoltHitboxBody body) {
    _body = body;
    _count = Mathf.Min(body._hitboxes.Length, _hbwtl.Length);

    if (body._proximity) {
      _wtl = body._proximity.transform.worldToLocalMatrix;
      _ltw = body._proximity.transform.localToWorldMatrix;
    }

    for (int i = 0; i < _count; ++i) {
      _hbwtl[i] = body._hitboxes[i].transform.worldToLocalMatrix;
      _hbltw[i] = body._hitboxes[i].transform.localToWorldMatrix;
    }
  }

  public void Dispose () {
    _body = null;
    _count = 0;
    _wtl = Matrix4x4.identity;
    _ltw = Matrix4x4.identity;

    Array.Clear(_hbwtl, 0, _hbwtl.Length);
    Array.Clear(_hbltw, 0, _hbltw.Length);

    _pool.Release(this);
  }

  public void OverlapSphere (Vector3 center, float radius, BoltPhysicsHits hits) {
    if (!_body) {
      return;
    }

    if (_body._proximity) {
      if (_body._proximity.OverlapSphere(ref _wtl, center, radius)) {
        hits.AddHit(_body, _body._proximity, (center - _ltw.MultiplyPoint(Vector3.zero)).magnitude);
      } else {
        return;
      }
    }

    for (int i = 0; i < _body._hitboxes.Length; ++i) {
      BoltHitbox hitbox = _body._hitboxes[i];

      if (hitbox.OverlapSphere(ref _hbwtl[i], center, radius)) {
        hits.AddHit(_body, hitbox, (center - _hbltw[i].MultiplyPoint(Vector3.zero)).magnitude);
      }
    }
  }

  public void Raycast (Vector3 origin, Vector3 direction, BoltPhysicsHits hits) {
    if (!_body) {
      return;
    }

    float distance = float.NegativeInfinity;

    if (_body._proximity) {
      if (_body._proximity.Raycast(ref _wtl, origin, direction, out distance)) {
        hits.AddHit(_body, _body._proximity, distance);
      } else {
        return;
      }
    }

    for (int i = 0; i < _body._hitboxes.Length; ++i) {
      BoltHitbox hitbox = _body._hitboxes[i];

      if (hitbox.Raycast(ref _hbwtl[i], origin, direction, out distance)) {
        hits.AddHit(_body, hitbox, distance);
      }
    }
  }

  public void Draw () {
#if DEBUG
    if (!_body) {
      return;
    }

    if (_body._proximity) {
      _body._proximity.Draw(_ltw);
    }

    for (int i = 0; i < _count; ++i) {
      _body._hitboxes[i].Draw(_hbltw[i]);
    }
#endif
  }

  public static BoltHitboxBodySnapshot Create (BoltHitboxBody body) {
    BoltHitboxBodySnapshot sn = _pool.Acquire();
    sn.Snapshot(body);
    return sn;
  }
}
