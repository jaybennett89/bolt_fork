using System;

/// <summary>
/// Interface all command factories must implement
/// </summary>
public interface IBoltCommandFactory {

  /// <summary>
  /// The type of the command this factory creates. This must be unique across the application.
  /// </summary>
  Type commandType { get; }

  /// <summary>
  /// The id of the command this factory creates. This must be unique across the application.
  /// </summary>
  ushort commandId { get; }

  /// <summary>
  /// Create a new command of the type returned by commandType
  /// </summary>
  BoltCommand Create ();
}
