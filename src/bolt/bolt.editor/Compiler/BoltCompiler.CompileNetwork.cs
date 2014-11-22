using Bolt.Compiler;
using System.Linq;

partial class BoltCompiler {
  static void CompileNetwork(BoltCompilerOperation op) {
    using (BoltSourceFile file = new BoltSourceFile(op.networkFilePath)) {
      file.EmitScope("namespace BoltInternal", () => {
        file.EmitScope("public static class BoltNetworkInternal_User", () => {
          file.EmitScope("public static void EnvironmentSetup()", () => {
            // Commands
            foreach (var def in op.project.Commands) {
              CommandDecorator dec;

              dec = new CommandDecorator();
              dec.Definition = def;

              file.EmitLine("Bolt.Factory.Register({0}.Instance);", dec.NameMeta);
            }

            // Events
            foreach (var def in op.project.Events) {
              EventDecorator dec;

              dec = new EventDecorator();
              dec.Definition = def;

              file.EmitLine("Bolt.Factory.Register(new {0}());", dec.FactoryName);
            }

            // State
            foreach (var def in op.project.States.Where(x => !x.IsAbstract)) {
              StateDecorator dec;

              dec = new StateDecorator();
              dec.Definition = def;

              file.EmitLine("Bolt.Factory.Register(new {0}());", dec.FactoryName);
            }
          });

          file.EmitScope("public static void EnvironmentReset()", () => {

          });
        });
      });
    }
  }
}
