using Bolt;
using System;
using System.Collections.Generic;

/// <summary>
/// Describes a hit to a BoltHitbox on a BoltHitboxBody
/// </summary>
/// <example>
/// *Example:* Logging the details of a BoltPhysicsHit object.
/// 
/// ```csharp
/// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
///   if(entity.isOwner) {
///     using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
///       cmd.ServerFrame))0 {
///       
///       if(hit.count > 0) {
///         BoltPhysicsHit hit = hits.GetHit(0);   
///         Debug.Log(string.Format("[HIT] Target={0}, Distance={1}, HitArea={2}", hit.body.gameObject.name, hit.distance, hit.hitbox.hitboxType);
///       }
///     }
///   }
/// }
/// ```
/// </example>
[Documentation]
public struct BoltPhysicsHit {
  /// <summary>
  /// The distance away from the origin of the ray
  /// </summary>
  public float distance;

  /// <summary>
  /// Which hitbox was hit
  /// </summary>
  public BoltHitbox hitbox;

  /// <summary>
  /// The body which was hit
  /// </summary>
  public BoltHitboxBody body;
}

/// <summary>
/// Container for a group of BoltPhysicsHits
/// </summary>
/// <example>
/// *Example:* Using ```BoltNetwork.RaycastAll()``` to detect hit events and processing the BoltPhysicsHits object that is returned.
/// 
/// ```csharp
/// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
///   if(entity.isOwner) {
///     using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
///       cmd.ServerFrame)) {
///       var hit = hits.GetHit(0);
///       var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
///       
///       if(targetEntity.StateIs&ltILivingEntity&gt()) {
///         targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
///       }
///     }
///   }
/// }
/// ```
/// </example>
[Documentation]
public class BoltPhysicsHits : BoltObject, IDisposable {
  internal static readonly BoltObjectPool<BoltPhysicsHits> _pool = new BoltObjectPool<BoltPhysicsHits>();
  internal List<BoltPhysicsHit> _hits = new List<BoltPhysicsHit>();

  /// <summary>
  /// How many hits we have in the collection
  /// </summary>
  /// <example>
  /// *Example:* Using the hit count to iterate through all hits
  /// 
  /// ```csharp
  /// void OnOwner(PlayerCommand cmd, BoltEntity entity) {
  ///   if(entity.isOwner) {
  ///     using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
  ///       cmd.ServerFrame)) {
  ///       
  ///       for(int i = 0; i < hits.count; ++i) {
  ///         var hit = hits.GetHit(i);
  ///         var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///       
  ///         if(targetEntity.StateIs&ltILivingEntity&gt()) {
  ///           targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
  ///         }
  ///       }
  /// }
  /// ```
  /// </example>
  public int count {
    get { return _hits.Count; }
  }

  /// <summary>
  /// Array indexing of the hits in this object
  /// </summary>
  /// <param name="index">Index position</param>
  /// <returns>The BoltPhysicsHit at the given index</returns>
  /// <example>
  /// *Example:* Using the array indexing to get the first object hit by a weapon firing raycast.
  /// 
  /// ```csharp
  /// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
  ///   if(entity.isOwner) {
  ///     using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
  ///       cmd.ServerFrame))0 {
  ///       
  ///       if(hit.count > 0) {
  ///         var hit = hits[0];
  ///         var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///       
  ///         if(targetEntity.StateIs&ltILivingEntity&gt()) {
  ///           targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
  ///         }
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public BoltPhysicsHit this[int index] {
    get { return _hits[index]; }
  }

  /// <summary>
  /// Get the hit at a specific index
  /// </summary>
  /// <param name="index">Index position</param>
  /// <returns>The BoltPhysicsHit at the given index</returns>
  /// <example>
  /// *Example:* Using the GetHit method to find the first object hit by a weapon firing raycast.
  /// 
  /// ```csharp
  /// void FireWeaponOwner(PlayerCommand cmd, BoltEntity entity) {
  ///   if(entity.isOwner) {
  ///     using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(new Ray(entity.transform.position, cmd.Input.targetPos),
  ///       cmd.ServerFrame))0 {
  ///       
  ///       if(hit.count > 0) {
  ///         var hit = hits.GetHit(0);
  ///         var targetEntity = hit.body.GetComponent&ltBoltEntity&gt();
  ///       
  ///         if(targetEntity.StateIs&ltILivingEntity&gt()) {
  ///           targetEntity.GetState&ltILivingEntity&gt().Modify().HP -= activeWeapon.damage; 
  ///         }
  ///       }
  ///     }
  ///   }
  /// }
  /// ```
  /// </example>
  public BoltPhysicsHit GetHit (int index) {
    return _hits[index];
  }

  /// <summary>
  /// Implementing the IDisposable interface to allow "using" syntax.
  /// </summary>
  /// <example>
  /// *Example:* Implementing the Disponse() method allows BoltPhysicsHits to be in a "using" block.
  /// 
  /// ```csharp
  /// void DoRaycast(Ray ray) {
  ///   using(BoltPhysicsHits hits = BoltNetwork.RaycastAll(ray)) {
  ///     // the hits variable will be automatically disposed at the end of this block
  ///   }
  /// }
  /// ```
  /// </example>
  public void Dispose () {
    _hits.Clear();
    _pool.Release(this);
  }

  internal void AddHit (BoltHitboxBody body, BoltHitbox hitbox, float distance) {
    _hits.Add(new BoltPhysicsHit { body = body, hitbox = hitbox, distance = distance });
  }
}
