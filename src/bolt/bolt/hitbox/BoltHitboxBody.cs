using Bolt;
using UnityEngine;

/// <summary>
/// Defines a body of hitboxes to be tracked 
/// </summary>
/// <example>
/// *Example:* Adding a hitbox body to a character pre-configured with BoltHitbox components
/// 
/// ```csharp
/// void AddHitboxBody(BoltEntity entity) { 
///   BoltHitbox[] hitboxes = entity.GetComponentsInChildren&ltBoltHitbox&gt();
///   
///   BoltHitboxBody body = entity.AddComponent&ltBoltHitboxBody&gt();
///   body.hitboxes = hitboxes;
/// }
/// ```
/// </example>
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

  /// <summary>
  /// An array of hitbox components that compose this body
  /// </summary>
  /// <example>
  /// *Example:* Finding all hitbox components on an entity and adding them to a hitbox body
  /// 
  /// ```csharp
  /// void AddHitboxBody(BoltEntity entity) { 
  ///   BoltHitbox[] hitboxes = entity.gameObject.GetComponentsInChildren&ltBoltHitbox&gt();
  ///   
  ///   BoltHitboxBody body = entity.AddComponent&ltBoltHitboxBody&gt();
  ///   body.hitboxes = hitboxes;
  /// }
  /// ```
  /// </example>
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
