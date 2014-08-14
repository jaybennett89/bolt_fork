using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

partial class BoltCompiler {
  public static void CompileMaps (BoltCompilerOperation op) {

    using (BoltSourceFile file = new BoltSourceFile(op.mapsFilePath)) {

      file.EmitScope("public enum BoltMapNames : int", () => {
        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i) {
          var s = EditorBuildSettings.scenes[i];

          if (s.enabled) {
            file.EmitLine("{0} = {1},", Path.GetFileNameWithoutExtension(s.path), i);
          }
        }
      });
    }
  }
}
