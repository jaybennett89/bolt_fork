using System;

/// <summary>
/// Attribute which lets you set the execution order of a script from C#
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BoltExecutionOrderAttribute : Attribute {
  readonly int _executionOrder;

  /// <summary>
  /// </summary>
  /// <param name="order">Which execution order this script should have</param>
  public BoltExecutionOrderAttribute (int order) {
    _executionOrder = order;
  }

  /// <summary>
  /// The execution order
  /// </summary>
  public int executionOrder {
    get { return _executionOrder; }
  }
}