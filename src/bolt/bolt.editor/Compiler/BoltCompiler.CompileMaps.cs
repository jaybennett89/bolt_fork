using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

partial class BoltCompiler {
  class SceneComparer : System.Collections.Generic.IEqualityComparer<EditorBuildSettingsScene> {
    public bool Equals (EditorBuildSettingsScene x, EditorBuildSettingsScene y) {
      return x.path.CompareTo(y.path) == 0;
    }

    public int GetHashCode (EditorBuildSettingsScene obj) {
      return obj.path.GetHashCode();
    }
  }

  public static void CompileMaps (BoltCompilerOperation op) {
    var scenes = EditorBuildSettings.scenes.Distinct(new SceneComparer()).ToArray();

    using (BoltSourceFile file = new BoltSourceFile(op.mapsFilePath)) {
      file.EmitScope("public static class BoltScenes", () => {
        for (int i = 0; i < scenes.Length; ++i) {
          var s = scenes[i];
          file.EmitLine("public const string {0} = \"{0}\";", Path.GetFileNameWithoutExtension(s.path));
        }
      });

      file.EmitScope("[System.Obsolete(\"Use BoltScenes instead\")] public static class BoltMaps", () => {
        for (int i = 0; i < scenes.Length; ++i) {
          var s = scenes[i];
          file.EmitLine("public const string {0} = \"{0}\";", Path.GetFileNameWithoutExtension(s.path));
        }
      });

      file.EmitScope("[System.Obsolete(\"Use BoltScenes instead\")] public enum BoltMapNames", () => {
        for (int i = 0; i < scenes.Length; ++i) {
          var s = scenes[i];
          file.EmitLine("{0} = {1},", Path.GetFileNameWithoutExtension(s.path), i);
        }
      });
    }
  }
}
