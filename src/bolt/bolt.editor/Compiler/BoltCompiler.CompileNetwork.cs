using Bolt.Compiler;
using System.Linq;
using System.Reflection;

partial class BoltCompiler {
  static void CompileNetwork(BoltCompilerOperation op) {
    mode = BoltCompilerMode.Network;

    using (BoltSourceFile file = new BoltSourceFile(op.networkFilePath)) {
      string src = Assembly.GetExecutingAssembly().GetResourceText("bolt.editor.Resources.BoltNetwork.cs");

      // event registration
      src = src.Replace("//EVENTS",
        op.events.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      // state registration
      src = src.Replace("//STATE",
        op.project.States.Where(x => !x.IsAbstract).Select(x => {
          StateDecorator dec = new StateDecorator();
          dec.Definition = x;

          return string.Format("BoltFactory.Register(new {0}());", dec.FactoryName);

        }).Join("\r\n")
        //op.states.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      // command registration
      src = src.Replace("//COMMANDS",
        op.project.Commands.Select(x => {
          CommandDecorator dec = new CommandDecorator();
          dec.Definition = x;

          return string.Format("BoltFactory.Register(new {0}());", dec.FactoryName);

        }).Join("\r\n")
        //op.commands.Select(x => string.Format("BoltFactory.Register(new {0}());", x.factoryName)).Join("\r\n")
      );

      file.Emit(src);
    }
  }
}
