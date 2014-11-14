﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UE = UnityEngine;

public partial class Player : IDisposable {
  public const byte TEAM_RED = 1;
  public const byte TEAM_BLUE = 2;

  public string name;
  public BoltEntity entity;
  public BoltConnection connection;

  public IPlayerState state {
    get { return entity.GetState<IPlayerState>(); }
  }

  public bool isServer {
    get { return connection == null; }
  }

  public Player() {
    players.Add(this);
  }

  public void Kill() {
    if (entity) {
      using (var mod = state.Modify()) {
        mod.Dead = true;
        mod.respawnFrame = BoltNetwork.serverFrame + (15 * BoltNetwork.framesPerSecond);
      }

      using (var ev = LogEvent.Raise(Bolt.GlobalTargets.Everyone)) {
        ev.message = name + " died";
      }
    }
  }

  internal void Spawn() {
    if (entity) {
      using (var mod = state.Modify()) {
        mod.Dead = false;
        mod.health = 100;
      }

      // teleport
      entity.transform.position = RandomSpawn();
    }
  }
  public void Dispose() {
    players.Remove(this);

    // destroy
    if (entity) {
      BoltNetwork.Destroy(entity.gameObject);
    }

    // while we have a team difference of more then 1 player
    while (Mathf.Abs(redPlayers.Count() - bluePlayers.Count()) > 1) {
      if (redPlayers.Count() < bluePlayers.Count()) {
        var player = bluePlayers.First();
        player.Kill();
        player.state.Modify().team = TEAM_RED;
      }
      else {
        var player = redPlayers.First();
        player.Kill();
        player.state.Modify().team = TEAM_BLUE;
      }
    }
  }

  public void InstantiateEntity() {
    float x = UE.Random.Range(-32f, +32f);
    float z = UE.Random.Range(-32f, +32f);

    entity = BoltNetwork.Instantiate(BoltPrefabs.Player, new TestToken(), RandomSpawn(), Quaternion.identity);

    using (var mod = state.Modify()) {
      mod.name = name;
      mod.team =
        redPlayers.Count() >= bluePlayers.Count()
        ? TEAM_BLUE
        : TEAM_RED;

      if (isServer) {
        entity.TakeControl(new TestToken());
      }
      else {
        entity.AssignControl(connection, new TestToken());
      }
    }

    Spawn();
  }

}

partial class Player {
  static List<Player> players = new List<Player>();

  public static IEnumerable<Player> redPlayers {
    get { return players.Where(x => x.entity && x.state.team == TEAM_RED); }
  }

  public static IEnumerable<Player> bluePlayers {
    get { return players.Where(x => x.entity && x.state.team == TEAM_BLUE); }
  }

  public static IEnumerable<Player> allPlayers {
    get { return players; }
  }

  public static bool serverIsPlaying {
    get { return serverPlayer != null; }
  }

  public static Player serverPlayer {
    get;
    private set;
  }

  public static void CreateServerPlayer() {
    serverPlayer = new Player();
  }

  static Vector3 RandomSpawn() {
    float x = UE.Random.Range(-32f, +32f);
    float z = UE.Random.Range(-32f, +32f);
    return new Vector3(x, 32f, z);
  }

}