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

  public void Dispose() {
    players.Remove(this);

    // destroy
    if (entity) {
      BoltNetwork.Destroy(entity);
    }

    // while we have a team difference of more then 1 player
    while (Mathf.Abs(redPlayers.Count() - bluePlayers.Count()) > 1) {
      if (redPlayers.Count() < bluePlayers.Count()) {
        bluePlayers.First().state.team = TEAM_RED;
      }
      else {
        redPlayers.First().state.team = TEAM_BLUE;
      }
    }
  }

  public void InstantiateEntity() {
    float x = UE.Random.Range(-32f, +32f);
    float z = UE.Random.Range(-32f, +32f);

    entity = BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(x, 32f, z), Quaternion.identity);
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

}