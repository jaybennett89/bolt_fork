using System;
using UnityEngine;

enum BoltCompilerMode {
  Event,
  State,
  Mecanim,
  Prefabs,
  Network
}

static partial class BoltCompiler {

  public static BoltCompilerMode mode = BoltCompilerMode.Event;

  public static void Run (BoltCompilerOperation op) {
    CompileMecanim(op);
    CompileEvents(op);
    CompileStates(op);
    CompilePrefabs(op);
    CompileNetwork(op);
    CompileMaps(op);
    CompileCommands(op);
    CompileAssemblyInfo(op);
  }

  static void EmitFileHeader (BoltSourceFile file) {
    file.EmitLine("using System;");
    file.EmitLine("using System.Collections.Generic;");
    file.EmitLine("using UdpKit;");
    file.EmitLine();

    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Int, typeof(int).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Bool, typeof(bool).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Long, typeof(long).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.UShort, typeof(ushort).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Float, typeof(float).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Vector2, typeof(Vector2).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Vector3, typeof(Vector3).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Vector4, typeof(Vector4).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Quaternion, typeof(Quaternion).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Entity, typeof(BoltEntity).CSharpName());
    
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.UniqueId, typeof(BoltUniqueId).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Custom, typeof(IBoltStateProperty).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Mecanim, typeof(IBoltStateProperty).CSharpName());
    file.EmitLine("using {0} = {1};", BoltAssetPropertyType.Transform, typeof(IBoltStateProperty).CSharpName());
    file.EmitLine();
  }

  public static void EmitWrite (BoltSourceFile file, BoltAssetProperty p, string expr, string connexpr) {
    EmitSerializer(file, p, expr, connexpr, true);
  }

  public static void EmitRead (BoltSourceFile file, BoltAssetProperty p, string expr, string connexpr) {
    EmitSerializer(file, p, expr, connexpr, false);
  }

  static void EmitSerializer (BoltSourceFile file, BoltAssetProperty p, string valexpr, string connexpr, bool write) {
    string expr = string.Format(valexpr, p.name);

    switch (p.type) {

      case BoltAssetPropertyType.UniqueId:
        if (write) {
          file.EmitLine("stream.WriteUniqueId({0});", expr);
        } else {
          
          file.EmitLine("{0} = stream.ReadUniqueId();", expr);
        }
        break;

      case BoltAssetPropertyType.ByteArray:
        if (write) {
          file.EmitLine("stream.WriteByteArraySimple({0}, 256);", expr);
        }
        else {
          file.EmitLine("{0} = stream.ReadByteArraySimple();", expr);
        }
        break;

      case BoltAssetPropertyType.Entity:
        if (write) {
          file.EmitLine("stream.WriteEntity({0}, {1});", expr, connexpr);
        }
        else {
          file.EmitLine("{0} = stream.ReadEntity({1});", expr, connexpr);
        }
        break;

      case BoltAssetPropertyType.Bool:
        if (write) {
          file.EmitLine("stream.WriteBool({0});", expr);
        }
        else {
          file.EmitLine("{0} = stream.ReadBool();", expr);
        }
        break;

      case BoltAssetPropertyType.Byte: EmitIntSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.UShort: EmitIntSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Int: EmitIntSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Long: EmitIntSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Float: EmitFloatSerializer(file, p.floatSettings.compression, expr, write); break;
      case BoltAssetPropertyType.Vector2: EmitVectorSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Vector3: EmitVectorSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Vector4: EmitVectorSerializer(file, p, expr, write); break;
      case BoltAssetPropertyType.Quaternion: EmitQuaternionSerializer(file, p, expr, write); break;

      case BoltAssetPropertyType.String:
        int length = int.MaxValue;

        if (p.stringSettings.maxLength > 0) {
          length = p.stringSettings.maxLength;
        }

        if (write) {
          file.EmitLine("stream.WriteString({0}, System.Text.Encoding.{1}, {2});", expr, p.stringSettings.encoding, length);
        }
        else {
          file.EmitLine("{0} = stream.ReadString(System.Text.Encoding.{1});", expr, p.stringSettings.encoding);
        }

        break;

      case BoltAssetPropertyType.Custom:
      case BoltAssetPropertyType.Mecanim:
      case BoltAssetPropertyType.Transform:
        if (write) {
          file.EmitLine("{0}.Pack(info, stream);", expr);
        }
        else {
          file.EmitLine("{0}.Read(info, stream);", expr);
        }
        break;
    }
  }

  static void EmitQuaternionSerializer (BoltSourceFile file, BoltAssetProperty p, string expr, bool write) {
    var axes = BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z | BoltAssetAxes.W;
    EmitVectorSerializer(file, p.type, axes, p.quaternionSettings.compression, expr, write);
  }

  static void EmitVectorSerializer (BoltSourceFile file, BoltAssetProperty p, string expr, bool write) {
    EmitVectorSerializer(file, p.type, p.vectorSettings.axes, p.vectorSettings.compression, expr, write);
  }

  static void EmitVectorSerializer (BoltSourceFile file, BoltAssetPropertyType type, BoltAssetAxes axes, BoltAssetFloatCompression compression, string expr, bool write) {
    switch (type) {
      case BoltAssetPropertyType.Vector2: axes &= (BoltAssetAxes.X | BoltAssetAxes.Y); break;
      case BoltAssetPropertyType.Vector3: axes &= (BoltAssetAxes.X | BoltAssetAxes.Y | BoltAssetAxes.Z); break;
    }

    EmitVectorAxis(file, axes, BoltAssetAxes.X, compression, expr, write);
    EmitVectorAxis(file, axes, BoltAssetAxes.Y, compression, expr, write);
    EmitVectorAxis(file, axes, BoltAssetAxes.Z, compression, expr, write);
    EmitVectorAxis(file, axes, BoltAssetAxes.W, compression, expr, write);
  }

  static void EmitVectorAxis (BoltSourceFile file, BoltAssetAxes mask, BoltAssetAxes axis, BoltAssetFloatCompression compression, string expr, bool write) {
    if ((mask & axis) == axis) {
      EmitFloatSerializer(file, compression, expr + "." + axis.ToString().ToLowerInvariant(), write);
    }
  }

  static void EmitFloatSerializer (BoltSourceFile file, BoltAssetFloatCompression compression, string expr, bool write) {
    if (compression == BoltAssetFloatCompression.None) {
      if (write) {
        file.EmitLine("stream.WriteFloat({0});", expr);
      }
      else {
        file.EmitLine("{0} = stream.ReadFloat();", expr);
      }
    }
    else if (compression == BoltAssetFloatCompression.Half) {
      if (write) {
        file.EmitLine("stream.WriteHalf({0});", expr);
      }
      else {
        file.EmitLine("{0} = stream.ReadHalf();", expr);
      }
    }
    else {
      if (write) {
        file.EmitLine("stream.WriteByte(BoltFloatCompression.{1}({0}));", expr, compression);
      }
      else {
        file.EmitLine("{0} = BoltFloatCompression.{1}(stream.ReadByte());", expr, compression);
      }
    }
  }

  static void EmitIntSerializer (BoltSourceFile file, BoltAssetProperty p, string expr, bool write) {
    int bits = 8;

    switch (p.type) {
      case BoltAssetPropertyType.Byte: bits = Mathf.Clamp(p.intSettings.byteBits, 1, 8); break;
      case BoltAssetPropertyType.UShort: bits = Mathf.Clamp(p.intSettings.shortBits, 1, 16); break;
      case BoltAssetPropertyType.Int: bits = Mathf.Clamp(p.intSettings.intBits, 1, 32); break;
      case BoltAssetPropertyType.Long: bits = Mathf.Clamp(p.intSettings.longBits, 1, 64); break;
    }

    if (write) {
      file.EmitLine("stream.Write{2}({0}, {1});", expr, bits, p.type);
    }
    else {
      file.EmitLine("{0} = stream.Read{2}({1});", expr, bits, p.type);
    }
  }

  static void EmitProxyControllerCheck (BoltSourceFile file, BoltAssetSyncTarget target, Action callback) {
    bool sendToProxy = (target & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy;
    bool sendToController = (target & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller;

    if (sendToProxy && sendToController) {
      callback();
    }
    else {
      if (sendToProxy) file.EmitScope("if (isProxy)", callback);
      if (sendToController) file.EmitScope("if (isController)", callback);
    }
  }
}
