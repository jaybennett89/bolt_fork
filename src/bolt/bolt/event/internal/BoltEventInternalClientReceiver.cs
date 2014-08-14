using UnityEngine;

class BoltEventClientReceiver : BoltEventGlobalReceiverInternal, ILoadMapReceiver, ILoadMapDoneReceiver {
  void ILoadMapDoneReceiver.OnEvent (ILoadMapDone evnt, BoltConnection connection) {
    Assert.True(BoltCore.isClient);
    Assert.False(BoltCore.isServer);

#if BOLT_CLIENT
    connection._loadedMap = evnt.op;
    connection.TriggerServerLoadedMapCallback();
#endif
  }

  void ILoadMapReceiver.OnEvent (ILoadMap evnt, BoltConnection connection) {
    Assert.True(BoltCore.isClient);
    Assert.False(BoltCore.isServer);

#if BOLT_CLIENT
    // load target
    connection._loadedMapTarget = evnt.op;
    connection._flags |= BoltConnection.FLAG_LOADING_MAP;

    // start loading
    BoltCore.LoadMapInternal(evnt.op);
#endif
  }
}
