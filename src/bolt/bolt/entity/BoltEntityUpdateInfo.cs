using System;

/// <summary>
/// Contains information about the current entity update which is being written or read
/// </summary>
public struct BoltEntityUpdateInfo {
  /// <summary>
  /// Which frame this update is for
  /// </summary>
  public int frame { get; internal set; }

  /// <summary>
  /// If this is the first update of the entity or a subsequente one
  /// </summary>
  public bool first { get; internal set; }

  /// <summary>
  /// If packing the connection this update is going to.
  /// If reading the connection the update was received from.
  /// </summary>
  public BoltConnection connection { get; internal set; }
}