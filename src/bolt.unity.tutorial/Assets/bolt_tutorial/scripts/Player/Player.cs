using UnityEngine;
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
    get { return entity.GetBoltState<IPlayerState>(); }
  }

  public bool isServer {
    get { return connection == null; }
  }

  public Player() {
    players.Add(this);
  }

  public void Kill() {
    if (entity) {
      state.dead = true;
      state.respawnFrame = BoltNetwork.serverFrame + (15 * BoltNetwork.framesPerSecond);

      ILogEvent ev;

      ev = BoltFactory.NewEvent<ILogEvent>();
      ev.message = name + " died";

      BoltNetwork.Raise(ev);
    }
  }

  internal void Spawn() {
    if (entity) {
      // not dead anymore
      state.dead = false;
      state.health = 100;

      // teleport
      entity.Teleport(RandomSpawn(), Quaternion.identity);
    }
  }
  public void Dispose() {
    players.Remove(this);

    // destroy
    if (entity) {
      BoltNetwork.Destroy(entity);
    }

    // while we have a team difference of more then 1 player
    while (Mathf.Abs(redPlayers.Count() - bluePlayers.Count()) > 1) {
      if (redPlayers.Count() < bluePlayers.Count()) {
        var player = bluePlayers.First();
        player.Kill();
        player.state.team = TEAM_RED;
      }
      else {
        var player = redPlayers.First();
        player.Kill();
        player.state.team = TEAM_BLUE;
      }
    }
  }

  public void GiveControl(BoltEntity entity) {
    if (connection == null) {
      entity.TakeControl();
    }
    else {
      entity.GiveControl(connection);
    }
  }

  public void InstantiateEntity() {
    float x = UE.Random.Range(-32f, +32f);
    float z = UE.Random.Range(-32f, +32f);

    entity = BoltNetwork.Instantiate(BoltPrefabs.Player, RandomSpawn(), Quaternion.identity);
    entity.GetBoltState<IPlayerState>().name = name;

    state.team =
      redPlayers.Count() >= bluePlayers.Count()
      ? TEAM_BLUE
      : TEAM_RED;

    if (isServer) {
      entity.TakeControl();
    }
    else {
      entity.GiveControl(connection);
    }

    Spawn();

    ILogEvent ev;

    ev = BoltFactory.NewEvent<ILogEvent>();
    ev.message = name + " joined the game";

    BoltNetwork.Raise(ev);
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