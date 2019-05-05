using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// What type of shape to use in a bolt hitbox
/// </summary>
/// *Example:* Sorting the hitboxes in a body based on shape.
/// 
/// ```csharp
/// void ConfigureHitboxes(BoltHitboxBody body) {
///   foreach(BoltHitbox hitbox in body.hitboxes) {
///     switch(hitbox.hitboxShape) {
///       case BoltHitboxShape.Sphere: ConfigureSphere(hitbox); break;
///       case BoltHitboxShape.Box: ConfigureBox(hitbox); break;
///     }
///   }
/// }
/// ```
/// </example>
[Documentation]
public enum BoltHitboxShape {
  Box,
  Sphere
}
