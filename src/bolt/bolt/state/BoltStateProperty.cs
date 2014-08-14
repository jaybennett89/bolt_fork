using UdpKit;
using UnityEngine;

/// <summary>
/// Base interface for custom state properties
/// </summary>
public interface IBoltStateProperty : IBoltEntityCallbacks {
  /// <summary>
  /// Called for packing this property into a stream
  /// </summary>
  void Pack (BoltEntityUpdateInfo info, UdpStream stream);

  /// <summary>
  /// Called for unpacking this property from a stream
  /// </summary>
  void Read (BoltEntityUpdateInfo info, UdpStream stream);

  /// <summary>
  /// Called when a packet arrived for the entity for this state, but
  /// it contained no updated value for this property.
  /// </summary>
  /// <param name="info"></param>
  void Skip (BoltEntityUpdateInfo info);
}
