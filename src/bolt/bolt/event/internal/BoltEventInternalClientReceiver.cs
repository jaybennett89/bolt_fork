using UnityEngine;

class BoltEventClientReceiver : BoltEventGlobalReceiverInternal, ILoadMapReceiver, ILoadMapDoneReceiver {
  void ILoadMapDoneReceiver.OnEvent (ILoadMapDone evnt, BoltConnection connection) {
    Assert.True(BoltCore.isClient);
    Assert.False(BoltCore.isServer);

#if BOLT_CLIENT
    // finish remote state
    connection._remoteMapLoadState = connection._remoteMapLoadState.FinishLoad(evnt.map, BoltCore._mapLoadState.scene);
    connection.TriggerRemoteMapDoneCallbacks();
#endif
  }

  void ILoadMapReceiver.OnEvent (ILoadMap evnt, BoltConnection connection) {
    Assert.True(BoltCore.isClient);
    Assert.False(BoltCore.isServer);

#if BOLT_CLIENT
    // begin loading map locally
    BoltCore.LoadMapInternal(evnt.map);

    // set remote map load state
    connection._remoteMapLoadState = connection._remoteMapLoadState.BeginLoad(evnt.map);
#endif
  }
}
