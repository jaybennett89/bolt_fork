using System;

namespace Bolt {
  class SceneLoadChannel : BoltChannel {
    public override void Pack(BoltPacket packet) {
      var s = packet.stream;
      s.WriteBool(BoltCore._canReceiveEntities);
      s.WriteInt(BoltCore._localSceneLoading.Scene.Index, 16);
      s.WriteInt(BoltCore._localSceneLoading.Scene.Token, 16);
      s.WriteInt(BoltCore._localSceneLoading.State, 8);
    }

    public override void Read(BoltPacket packet) {
      var s = packet.stream;

      var canReceiveEntities = s.ReadBool();
      var sceneIndex = s.ReadInt(16);
      var sceneToken = s.ReadInt(16);
      var state = s.ReadInt(8);
      var scene = new Scene(sceneIndex, sceneToken);

      connection._canReceiveEntities = canReceiveEntities;

      if (connection._remoteSceneLoading.Scene != scene) {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = state;
      }
      else {
        connection._remoteSceneLoading.Scene = scene;
        connection._remoteSceneLoading.State = System.Math.Max(connection._remoteSceneLoading.State, state);
      }

      if (BoltCore.isClient) {
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

