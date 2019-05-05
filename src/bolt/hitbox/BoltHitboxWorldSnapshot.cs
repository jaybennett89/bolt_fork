using System;

class BoltHitboxWorldSnapshot : BoltObject, IDisposable {
  internal static readonly BoltObjectPool<BoltHitboxWorldSnapshot> _pool = new BoltObjectPool<BoltHitboxWorldSnapshot>();

  internal int _frame;
  internal BoltSingleList<BoltHitboxBodySnapshot> _bodySnapshots = new BoltSingleList<BoltHitboxBodySnapshot>();

  internal void Snapshot (BoltHitboxBody body) {
    _bodySnapshots.AddLast(BoltHitboxBodySnapshot.Create(body));
  }

  public void Dispose () {
    while (_bodySnapshots.count > 0) {
      _bodySnapshots.RemoveFirst().Dispose();
    }

    _frame = 0;
    _pool.Release(this);
  }

  public void Draw () {
#if DEBUG
    var it = _bodySnapshots.GetIterator();

    while (it.Next()) {
      it.val.Draw();
    }
#endif
  }
}
