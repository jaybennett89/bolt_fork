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
  }
}
