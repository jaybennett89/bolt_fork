using Bolt;
using UnityEngine;

/// <summary>
/// Defines a body of hitboxes to be tracked 
/// </summary>
[Documentation]
public class BoltHitboxBody : MonoBehaviour, IBoltListNode {
  [SerializeField]
  internal BoltHitbox _proximity;

  [SerializeField]
  internal BoltHitbox[] _hitboxes = new BoltHitbox[0];

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  public BoltHitbox proximity {
    get { return _proximity; }
    set { _proximity = value; }
  }

  public BoltHitbox[] hitboxes {
    get { return _hitboxes; }
    set { _hitboxes = value; }
  }

  void OnEnable () {
    BoltPhysics.RegisterBody(this);
  }

  void OnDisable () {
    BoltPhysics.UnregisterBody(this);
  }
}
