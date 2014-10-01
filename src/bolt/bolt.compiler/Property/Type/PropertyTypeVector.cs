﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Compiler {
  [ProtoContract]
  public enum VectorComponents {
    X = 0,
    Y = 1,
    Z = 2,
    W = 3
  }

  [ProtoContract]
  public class Axis {
    [ProtoMember(2)]
    public bool Enabled;

    [ProtoMember(1)]
    public VectorComponents Component;

    [ProtoMember(3)]
    public FloatCompression Compression;
  }

  [ProtoContract]
  public class PropertyTypeVector : PropertyType {
    [ProtoMember(1, OverwriteList = true)]
    public Axis[] Axes = new[] {
      new Axis { Component = VectorComponents.X, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Y, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.Z, Compression = FloatCompression.Default(), Enabled = true },
      new Axis { Component = VectorComponents.W, Compression = FloatCompression.Default(), Enabled = false },
    };

    //public override int ByteSize {
    //  get { return 16; }
    //}

    public override bool InterpolateAllowed {
      get { return true; }
    }

    public Axis this[VectorComponents component] {
      get {
        foreach (Axis axis in Axes) {
          if (axis.Component == component) {
            return axis;
          }
        }

        throw new ArgumentOutOfRangeException();
      }
    }

    //public override string ClrType {
    //  get {
    //    if (this[VectorComponents.W].Enabled) {
    //      return "UE.Vector4";
    //    }

    //    if (this[VectorComponents.Z].Enabled) {
    //      return "UE.Vector3";
    //    }

    //    return "UE.Vector2";
    //  }
    //}
  }
}
