﻿using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[BoltGlobalBehaviour(BoltNetworkModes.Server, "Level1")]
public class ServerCallbacks : Bolt.GlobalEventListener {
  public static bool ListenServer = true;

  void Awake() {
    if (ListenServer) {
      Player.CreateServerPlayer();
      Player.serverPlayer.name = "SERVER";
    }
  }

  void OnGUI() {
    GUILayout.Label("ENTITY COUNT: " + BoltNetwork.entities.Count() + " POLL TIME: " + Bolt.DebugInfo.PollTime);
  }

  void FixedUpdate() {
    foreach (Player p in Player.allPlayers) {
      // if we have an entity, it's dead but our spawn frame has passed
      if (p.entity && p.state.Dead && p.state.respawnFrame <= BoltNetwork.serverFrame) {
        p.Spawn();
      }
    }
  }

  public override void ConnectRequest(UdpKit.UdpEndPoint endpoint, Bolt.IProtocolToken token) {
    BoltNetwork.Accept(endpoint);
  }

  public override void Connected(BoltConnection c, Bolt.IProtocolToken token) {
    c.userToken = new Player();
    c.GetPlayer().connection = c;
    c.GetPlayer().name = "CLIENT:" + c.remoteEndPoint.Port;
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
}
