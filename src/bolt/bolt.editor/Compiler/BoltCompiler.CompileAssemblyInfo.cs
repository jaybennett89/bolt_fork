using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

partial class BoltCompiler {
  static void CompileAssemblyInfo (BoltCompilerOperation op) {
    using (BoltSourceFile file = new BoltSourceFile(op.assemblyInfoFilePath)) {
      file.EmitLine("using System.Reflection;");
      file.EmitLine("using System.Runtime.CompilerServices;");
      file.EmitLine("using System.Runtime.InteropServices;");
      file.EmitLine();
      file.EmitLine("[assembly: AssemblyTitle(\"bolt.user\")]");
      file.EmitLine("[assembly: Guid(\"bd29ff3d-20fc-49ac-8303-459b4d662c04\")]");
      file.EmitLine("[assembly: AssemblyVersion(\"0.2.1.4\")]");
      file.EmitLine("[assembly: AssemblyFileVersion(\"0.2.1.4\")]");
    }
  }
}
