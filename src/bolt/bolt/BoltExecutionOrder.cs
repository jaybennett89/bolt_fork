using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BoltExecutionOrderAttribute : Attribute {
  readonly int _executionOrder;

  public BoltExecutionOrderAttribute (int order) {
    _executionOrder = order;
  }

  public int executionOrder {
    get { return _executionOrder; }
  }
}