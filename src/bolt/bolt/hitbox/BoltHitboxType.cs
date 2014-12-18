using Bolt;

/// <summary>
/// The body area represented by a bolt hitbox
/// </summary>
/// <example>
/// *Example:* Modifying a base damage value depending on the area of the hit.
/// 
/// ```csharp
/// float CalculateDamage(BoltHitbox hit, float baseDamage) {
///   switch(hit.hitboxType) {
///     case BoltHitboxType.Head: return 2.0f * baseDamage;
///     
///     case BoltHitboxType.Leg:
///     case BoltHitboxType.UpperArm: return 0.7f * baseDamage;
///     
///     default: return baseDamage;
///   }
/// }
/// ```
/// </example>
[Documentation]
public enum BoltHitboxType {
  Unknown,
  Proximity,
  Body,
  Head,
  Throat,
  Shoulder,
  UpperArm,
  Forearm,
  Hand,
  Chest,
  Stomach,
  Pelvis,
  Buttocks,
  Thigh,
  Knee,
  Leg,
  Foot,
  Elbow,
  Ankle,
  Wrist,
  Finger,
  Heel
}
