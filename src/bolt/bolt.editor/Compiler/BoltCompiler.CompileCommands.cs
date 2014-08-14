using System.Linq;

partial class BoltCompiler {
  public static void CompileCommands (BoltCompilerOperation op) {
    using (BoltSourceFile file = new BoltSourceFile(op.commandsFilePath)) {
      EmitFileHeader(file);

      ushort id = 0;

      foreach (BoltCommandAsset asset in op.commands) {
        asset.id = id++;
      }

      foreach (BoltCommandAsset asset in op.commands) {
        // command class
        file.EmitScope("public class {0} : BoltCommand", asset.className, () => {
          file.EmitScope("public struct Input", () => {
            foreach (BoltAssetProperty p in asset.allInputProperties) {
              file.EmitLine("public {0} {1};", p.runtimeType, p.name);
            }
          });

          file.EmitScope("public struct State", () => {
            foreach (BoltAssetProperty p in asset.allStateProperties) {
              file.EmitLine("public {0} {1};", p.runtimeType, p.name);
            }
          });

          file.EmitLine("State _interpTo;");
          file.EmitLine("State _interpFrom;");

          file.EmitLine("public State state;");
          file.EmitLine("public Input input;");

          // constructor
          file.EmitLine("internal {0} () : base({1}) {{ }}", asset.className, asset.id);

          // packcommand method
          file.EmitScope("public override void PackInput (BoltConnection connection, UdpStream stream)", () => {
            foreach (BoltAssetProperty p in asset.allInputProperties.Where(x => x.assetSettingsCommand.synchronize)) {
              EmitWrite(file, p, "input.{0}", "connection");
            }
          });

          // readcommand method
          file.EmitScope("public override void ReadInput (BoltConnection connection, UdpStream stream)", () => {
            foreach (BoltAssetProperty p in asset.allInputProperties.Where(x => x.assetSettingsCommand.synchronize)) {
              EmitRead(file, p, "input.{0}", "connection");
            }
          });

          // packstate method
          file.EmitScope("public override void PackState (BoltConnection connection, UdpStream stream)", () => {
            foreach (BoltAssetProperty p in asset.allStateProperties.Where(x => x.assetSettingsCommand.synchronize)) {
              EmitWrite(file, p, "state.{0}", "connection");
            }
          });

          // readstate method
          file.EmitScope("public override void ReadState (BoltConnection connection, UdpStream stream)", () => {
            foreach (BoltAssetProperty p in asset.allStateProperties.Where(x => x.assetSettingsCommand.synchronize)) {
              EmitRead(file, p, "state.{0}", "connection");
            }
          });

          // dispose method
          file.EmitScope("public override void Interpolate ()", () => {

          });

          // dispose method
          file.EmitScope("public override void Dispose ()", () => {

          });

        });

        // factory class
        file.EmitScope("class {0} : IBoltCommandFactory", asset.factoryName, () => {
          file.EmitLine("public Type commandType {{ get {{ return typeof({0}); }} }}", asset.className);
          file.EmitLine("public ushort commandId {{ get {{ return {0}; }} }}", asset.id);
          file.EmitLine("public BoltCommand Create() {{ return new {0}(); }}", asset.className);
        });
      }
    }
  }
}
