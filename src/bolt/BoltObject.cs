/// <summary>
/// Utility base class for some common functionality inside of Bolt
/// </summary>
using Bolt;
public class BoltObject : IBoltListNode {
#if DEBUG
  internal bool _pooled = true;
#endif

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  [Documentation(Ignore = true)]
  public static implicit operator bool (BoltObject obj) {
    return obj != null;
  }
}
