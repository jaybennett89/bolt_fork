using System;

namespace Bolt {
  class SceneLoadChannel : BoltChannel {
    public override void Pack(BoltPacket packet) {
      var s = packet.stream;
      s.WriteInt(BoltCore._localSceneLoading.Scene.Index, 8);
      s.WriteInt(BoltCore._localSceneLoading.Scene.Token, 8);
      s.WriteInt((int)BoltCore._localSceneLoading.State, 8);
    }

    public override void Read(BoltPacket packet) {
      var s = packet.stream;

      Scene scene = new Scene(s.ReadInt(8), s.ReadInt(8));

      if (connection._remoteSceneLoading.Scene != scene) {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = s.ReadInt(8);
      }
      else {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = Math.Max(connection._remoteSceneLoading.State, s.ReadInt(8));
      }

      if (BoltCore.isClient) {
        // if the server is on a different map, go to it
        if (connection._remoteSceneLoading.Scene != BoltCore._localSceneLoading.Scene) {
          SceneLoadState localLoading;
          localLoading.Scene = connection._remoteSceneLoading.Scene;
          localLoading.State = SceneLoadState.STATE_LOADING;

          BoltCore.LoadSceneInternal(localLoading);
        }
      }
    }
  }
}

