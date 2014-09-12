using UnityEngine;
using System.Collections;

public class Player {
  public BoltEntity entity;

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
