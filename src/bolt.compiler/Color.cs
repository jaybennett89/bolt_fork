using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public struct Color4 {
    [ProtoMember(1)]
    public float R;
    [ProtoMember(2)]
    public float G;
    [ProtoMember(3)]
    public float B;
    [ProtoMember(4)]
    public float A;

    public Color4(float r, float g, float b, float a) {
      R = r;
      G = g;
      B = b;
      A = a;
    }
  }
}
