using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

partial class BoltCompiler {
  struct Scene {
    public string Name;
    public string Identifier;
  }

  class SceneComparer : System.Collections.Generic.IEqualityComparer<EditorBuildSettingsScene> {
    public bool Equals(EditorBuildSettingsScene x, EditorBuildSettingsScene y) {
      return x.path.CompareTo(y.path) == 0;
    }

    public int GetHashCode(EditorBuildSettingsScene obj) {
      return obj.path.GetHashCode();
    }
  }

  public static void CompileMaps(BoltCompilerOperation op) {
    var scenes = new List<Scene>();

    for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i) {
      if (EditorBuildSettings.scenes[i].enabled) {
        var name = Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path);

        scenes.Add(new Scene {
          Name = name,
          Identifier = BoltEditorUtilsInternal.CSharpIdentifier(name)
        });
      }
    }

    foreach (var group in scenes.GroupBy(x => x.Identifier)) {
      if (group.Count() > 1) {
        throw new BoltException("You have several scenes named '{0}' in the build settings.", group.Key);
      }
    }

    using (BoltSourceFile file = new BoltSourceFile(op.scenesFilePath)) {
      file.EmitLine("using System.Collections.Generic;");
      file.EmitScope("public static class BoltScenes", () => {
        file.EmitLine("static internal readonly Dictionary<string, int> nameLookup = new Dictionary<string, int>();");
        file.EmitLine("static internal readonly Dictionary<int, string> indexLookup = new Dictionary<int, string>();");

        file.EmitLine("static public IEnumerable<string> AllScenes { get { return nameLookup.Keys; } }");

        file.EmitScope("static BoltScenes()", () => {
          for (int n = 0; n < scenes.Count; ++n) {
            file.EmitLine("nameLookup.Add(\"{0}\", {1});", scenes[n].Name, n);
            file.EmitLine("indexLookup.Add({1}, \"{0}\");", scenes[n].Name, n);
          }
        });

        for (int n = 0; n < scenes.Count; ++n) {
          file.EmitLine("public const string {0} = \"{1}\";", scenes[n].Identifier, scenes[n].Name);
        }
      });

      file.EmitScope("namespace BoltInternal", () => {
        file.EmitScope("public static class BoltScenes_Internal", () => {
          file.EmitLine("static public int GetSceneIndex(string name) { return BoltScenes.nameLookup[name]; }");
          file.EmitLine("static public string GetSceneName(int index) { return BoltScenes.indexLookup[index]; }");
        });
      });
    }
  }
}
