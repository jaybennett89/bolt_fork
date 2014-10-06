using Bolt.Compiler;
using System.Linq;
using System.Reflection;

partial class BoltCompiler {
  static void CompileNetwork(BoltCompilerOperation op) {
    mode = BoltCompilerMode.Network;

    using (BoltSourceFile file = new BoltSourceFile(op.networkFilePath)) {
      string src = Assembly.GetExecutingAssembly().GetResourceText("bolt.editor.Resources.BoltNetwork.cs");

      // command registration
      src = src.Replace("//COMMANDS",
        op.project.Commands.Select(x => {
          CommandDecorator dec = new CommandDecorator();
          dec.Definition = x;

          return string.Format("Bolt.Factory.Register(new {0}());", dec.FactoryName);

        }).Join("\r\n")
      );

      // event registration
      src = src.Replace("//EVENTS",
        op.project.Events.Select(x => {
          EventDecorator dec = new EventDecorator();
          dec.Definition = x;

          return string.Format("Bolt.Factory.Register(new {0}());", dec.FactoryName);

        }).Join("\r\n")
      );

      // state registration
      src = src.Replace("//STATE",
        op.project.States.Where(x => !x.IsAbstract).Select(x => {
          StateDecorator dec = new StateDecorator();
          dec.Definition = x;

          return string.Format("Bolt.Factory.Register(new {0}());", dec.FactoryName);

        }).Join("\r\n")
      );

      file.Emit(src);
    }
  }
}
