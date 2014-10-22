using System;

namespace Bolt {
  class SceneLoadChannel : BoltChannel {
    public override void Pack(BoltPacket packet) {
      var s = packet.stream;
      s.WriteBool(BoltCore._canReceiveEntities);
      s.WriteInt(BoltCore._localSceneLoading.Scene.Index, 32);
      s.WriteInt(BoltCore._localSceneLoading.Scene.Token, 32);
      s.WriteInt((int)BoltCore._localSceneLoading.State, 32);
    }

    public override void Read(BoltPacket packet) {
      connection._canReceiveEntities = packet.stream.ReadBool();

      var s = packet.stream;

      Scene scene = new Scene(s.ReadInt(32), s.ReadInt(32));

      if (connection._remoteSceneLoading.Scene != scene) {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = s.ReadInt(32);
      }
      else {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = System.Math.Max(connection._remoteSceneLoading.State, s.ReadInt(32));
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

