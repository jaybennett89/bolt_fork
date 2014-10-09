﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server, BoltScenes.Level1)]
public class ServerCallbacks : BoltGlobalEventListener {
  public static bool ListenServer = true;

  void Awake() {
    if (ListenServer) {
      Player.CreateServerPlayer();
      Player.serverPlayer.name = "SERVER";
    }
  }

  void FixedUpdate() {
    foreach (Player p in Player.allPlayers) {
      // if we have an entity, it's dead but our spawn frame has passed
      if (p.entity && p.state.Dead && p.state.respawnFrame <= BoltNetwork.serverFrame) {
        p.Spawn();
      }
    }
  }

  public override void SceneLoadRemoteDone(BoltConnection connection) {
    connection.GetPlayer().InstantiateEntity();
  }

  public override void SceneLoadLocalDone(string map) {
    if (Player.serverIsPlaying) {
      Player.serverPlayer.InstantiateEntity();
    }
  }

  public override void SceneLoadLocalBegin(string map) {
    foreach (Player p in Player.allPlayers) {
      p.entity = null;
    }
  }

  public BoltEntity SpawnPlayerCharacter() {
    return BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(0, 15, 0), Quaternion.identity);
  }
}
