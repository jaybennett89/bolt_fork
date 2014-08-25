using UnityEngine;

class BoltEventServerReceiver : BoltEventGlobalReceiverInternal, ILoadMapDoneReceiver {
  public void OnEvent (ILoadMapDone evnt, BoltConnection connection) {
    Assert.True(BoltCore.isServer);
    Assert.False(BoltCore.isClient);

#if BOLT_SERVER
    // execute finish load on the state
    connection._remoteMapLoadState = connection._remoteMapLoadState.FinishLoad(evnt.map, BoltCore._mapLoadState.map);

    switch (connection._remoteMapLoadState.stage) {
      case MapLoadStage.Load:
        // we are loading a new map
        connection.SendMapLoadToRemote(); break;

      case MapLoadStage.LoadDone:
        // we are done loading and should try to trigger callback
        connection.TriggerRemoteMapDoneCallbacks(); break;
    }
#endif
  }
}
