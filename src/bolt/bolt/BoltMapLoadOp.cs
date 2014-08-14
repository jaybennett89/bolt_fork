using System;
using UnityEngine;

/// <summary>
/// Describes a map loading operation
/// </summary>
public struct BoltMapLoadOp : IEquatable<BoltMapLoadOp> {
  internal string map;
  internal int token;

  /// <summary>
  /// If this is a valid load operation
  /// </summary>
  public bool isValid {
    get { return map != null && token > 0; }
  }

  internal BoltMapLoadOp (string id, BoltMapLoadOp previous) {
    this.map = id;
    this.token = previous.token + 1;
  }

  public bool Equals (BoltMapLoadOp other) {
    return this.map == other.map && this.token == other.token;
  }
}