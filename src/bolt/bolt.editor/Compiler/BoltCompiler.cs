using System;
using System.IO;
using UnityEngine;

static partial class BoltCompiler {
  public static void Run(BoltCompilerOperation op) {
    CompileMaps(op);
    CompilePrefabs(op);
    CompileNetwork(op);
    CompileAssemblyInfo(op);
    op.project.GenerateCode(op.projectFilePath, BoltRuntimeSettings.instance.allowStatePropertySetters);
  }

  static void EmitFileHeader(BoltSourceFile file) {
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
    file.EmitLine();
  }
}
