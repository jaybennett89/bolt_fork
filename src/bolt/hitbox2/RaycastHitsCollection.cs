using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt {
  public struct RaycastHit {
    public HitboxBody Body;
    public Hitbox Hitbox;
    public float Distance;
  }

  public class RaycastHitsCollection : IDisposable {
    internal Ray Ray;
    internal int HitsCount;
    internal RaycastHit[] HitsArray = new RaycastHit[HitboxBodySnapshot.MAX_BODIES / 2];

    public int Count {
      get { return HitsCount; }
    }

    public RaycastHit this[int index] {
      get {
        if (index >= HitsCount) {
          throw new ArgumentOutOfRangeException();
        }

        return HitsArray[index];
      }
    }

    internal void Add(HitboxBody body, Hitbox hitbox, float distance) {
      if (HitsCount + 1 == HitsArray.Length) {
        Array.Resize(ref HitsArray, HitsArray.Length * 2);
      }

      HitsArray[HitsCount].Body = body;
      HitsArray[HitsCount].Hitbox = hitbox;
      HitsArray[HitsCount].Distance = distance;
    }

    public void Dispose() {

    }
  }
}
