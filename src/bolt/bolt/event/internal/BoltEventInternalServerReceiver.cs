using UnityEngine;

class BoltEventServerReceiver : BoltEventGlobalReceiverInternal, ILoadMapDoneReceiver {
  public void OnEvent (ILoadMapDone evnt, BoltConnection connection) {
    Assert.True(BoltCore.isServer);
    Assert.False(BoltCore.isClient);

#if BOLT_SERVER
    connection._loadedMap = evnt.op;
    connection.LoadMapOnClient(BoltCore._loadedMapTarget);
    connection.TriggerClientLoadedMapCallback();

    if (BoltMapLoader.isLoading == false) {
      connection.Raise<ILoadMapDone>(evt => evt.op = BoltCore._loadedMap);
    }
#endif
  }
}
