using System;
using System.Collections.Generic;
using Bolt.Compiler;
using System.Linq;

partial class BoltCompiler {
  static void EmitRegisterFactory<T>(BoltSourceFile file, IEnumerable<T> decorators) where T : AssetDecorator {
    foreach (var d in decorators) {
      file.EmitLine("Bolt.Factory.Register({0}.Instance);", d.NameMeta);
    }
  }

  static void CompileNetwork(BoltCompilerOperation op) {
    using (BoltSourceFile file = new BoltSourceFile(op.networkFilePath)) {
      file.EmitScope("namespace BoltInternal", () => {
        file.EmitScope("public static class BoltNetworkInternal_User", () => {
          file.EmitScope("public static void EnvironmentSetup()", () => {
            EmitRegisterFactory(file, op.project.Structs.Select(x => new ObjectDecorator(x)));
            EmitRegisterFactory(file, op.project.Commands.Select(x => new CommandDecorator(x)));
            EmitRegisterFactory(file, op.project.Events.Select(x => new EventDecorator(x)));
            EmitRegisterFactory(file, op.project.States.Select(x => new StateDecorator(x)));
          });

          file.EmitScope("public static void EnvironmentReset()", () => {

          });
        });
      });
    }
  }
}
