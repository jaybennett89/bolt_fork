using System.Linq;
using System.Reflection;

partial class BoltCompiler {
  static void CompileNetwork (BoltCompilerOperation op) {
    mode = BoltCompilerMode.Network;

    using (BoltSourceFile file = new BoltSourceFile(op.networkFilePath)) {
      string src = Assembly.GetExecutingAssembly().GetResourceText("bolt.editor.Resources.BoltNetwork.cs");

      // map loader
      if (BoltEditorUtils.hasPro) {
        src = src.Replace("//MAPLOADER", "return typeof(BoltMapLoaderPro);");
      } else {
        src = src.Replace("//MAPLOADER", "return typeof(BoltMapLoaderFree);");
      }

      // event registration
      src = src.Replace("//EVENTS",
        op.events.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      // state registration
      src = src.Replace("//STATE",
        op.states.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      // command registration
      src = src.Replace("//COMMANDS",
        op.commands.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      file.Emit(src);
    }
  }
}
