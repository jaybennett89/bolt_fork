using System;
using UnityEngine;

public enum BoltRandomFunction {
  PerlinNoise,
  SystemRandom
}

public enum BoltConnectionAcceptMode {
  Auto,
  Manual,
}

[Serializable]
public sealed class BoltConfig {
  public int framesPerSecond = 60;
  public bool autoCreateConsole = true;

  public int clientSendRate;
  public int clientDejitterDelay;
  public int clientDejitterDelayMin;
  public int clientDejitterDelayMax;

  public int serverSendRate;
  public int serverDejitterDelay;
  public int serverDejitterDelayMin;
  public int serverDejitterDelayMax;
  public int serverConnectionLimit;
  public BoltConnectionAcceptMode serverConnectionAcceptMode = BoltConnectionAcceptMode.Auto;

  public int commandDejitterDelay;
  public int commandQueueSize;

  public int commandDelayAllowed;
  public int commandRedundancy;
  public float commandPingMultiplier;

  public float simulatedLoss;
  public int simulatedPingMean;
  public int simulatedPingJitter;
  public BoltRandomFunction simulatedRandomFunction = BoltRandomFunction.PerlinNoise;

  public BoltConfig () {
    // sendrates of server/client
    serverSendRate = 3;
    clientSendRate = 3;

    // interpolation delay on client is based on server rate
    clientDejitterDelay = serverSendRate * 2;
    clientDejitterDelayMin = clientDejitterDelay - serverSendRate;
    clientDejitterDelayMax = clientDejitterDelay + serverSendRate;

    // interpolation delay on server is based on client rate
    serverDejitterDelay = clientSendRate * 2;
    serverDejitterDelayMin = serverDejitterDelay - clientSendRate;
    serverDejitterDelayMax = serverDejitterDelay + clientSendRate;

    // max clients connected to the server
    serverConnectionLimit = 64;

    // commands config
    commandRedundancy = 6;
    commandPingMultiplier = 1.25f;
    commandDejitterDelay = 3;
    commandDelayAllowed = clientDejitterDelay * 2;
    commandQueueSize = 60;
  }

  internal BoltConfig Clone () {
    return (BoltConfig) MemberwiseClone();
  }
}
