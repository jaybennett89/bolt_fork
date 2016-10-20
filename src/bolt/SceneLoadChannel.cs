using System;

namespace Bolt {
  class SceneLoadChannel : BoltChannel {
    public override void Pack(Packet packet) {
      var s = packet.UdpPacket;
      s.WriteBool(BoltCore._canReceiveEntities);

      var local = BoltCore._localSceneLoading;
      var remote = connection._remoteSceneLoading;

      s.WriteInt(local.State, 2);
      s.WriteInt(local.Scene.Index, 8);
      s.WriteInt(local.Scene.Sequence, 8);

      if (BoltNetwork.isServer) {
        if (s.WriteBool(local.Scene != remote.Scene)) {
          s.WriteToken(local.Token);
        }
      }
    }

    public override void Read(Packet packet) {
      var s = packet.UdpPacket;

      connection._canReceiveEntities = s.ReadBool();

      SceneLoadState local = BoltCore._localSceneLoading;
      SceneLoadState remote = new SceneLoadState();

      remote.State = s.ReadInt(2);
      remote.Scene = new Scene(s.ReadInt(8), s.ReadInt(8));

      if (BoltNetwork.isClient) {
        if (s.ReadBool()) {
          remote.Token = s.ReadToken();
        }
      }

      if (connection._remoteSceneLoading.Scene == remote.Scene) {
        remote.State = System.Math.Max(connection._remoteSceneLoading.State, remote.State);
      }

      connection._remoteSceneLoading = remote;

      if (BoltCore.isClient) {

        // if the scene the remote is loading is not the same as ours... we should switch
        if (remote.Scene != BoltCore._localSceneLoading.Scene) {

          // set the loading state
          remote.State = SceneLoadState.STATE_LOADING;

          // and begin loading
          BoltCore.LoadSceneInternal(remote);
        }
      }
    }
  }
}

