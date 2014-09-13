using UnityEngine;
using System.Collections;

public class Player {
  public string name;
  public BoltEntity entity;
  public BoltConnection connection;

  public IPlayerState state {
    get { return entity.GetBoltState<IPlayerState>(); }
  }

  public bool isServer {
    get { return connection == null; }
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

  public static Player GetPlayer(BoltConnection connection) {
    if (connection == null) {
      return serverPlayer;
    }

    return (Player) connection.userToken;
  }
}
