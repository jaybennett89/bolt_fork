using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt {
  partial struct HitboxBodySnapshot {
    public void Raycast(RaycastHitsCollection hits, HitboxBody body, int box, ref Matrix4x4 wtl, ref Matrix4x4 ltw) {
      var hitbox = body.HitboxArray[box];
      if (hitbox.Shape == HitboxShape.Sphere) {

      }
      else {
        var b = new Bounds(hitbox.Center, hitbox.Size);
        var d = 0f;

        if (b.IntersectRay(hits.Ray, out d)) {
          hits.Add(body, hitbox, d);
        }
      }
    }
  }
}
