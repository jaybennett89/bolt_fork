using UnityEngine;

/// <summary>
/// Defines a body of hitboxes to be tracked 
/// </summary>
public class BoltHitboxBody : MonoBehaviour, IBoltListNode {
  [SerializeField]
  internal BoltHitbox _proximity;

  [SerializeField]
  internal BoltHitbox[] _hitboxes = new BoltHitbox[0];

  object IBoltListNode.prev { get; set; }
  object IBoltListNode.next { get; set; }
  object IBoltListNode.list { get; set; }

  void OnEnable () {
    BoltPhysics.RegisterBody(this);
  }

  void OnDisable () {
    BoltPhysics.UnregisterBody(this);
  }
}
