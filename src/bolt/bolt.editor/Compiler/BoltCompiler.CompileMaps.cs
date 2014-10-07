using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

partial class BoltCompiler {
  struct SceneIndex {
    public int Index;
    public bool Enabled;
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
    var si = new SceneIndex[EditorBuildSettings.scenes.Length];

    for (int i = 0; i < si.Length; ++i) {
      if (EditorBuildSettings.scenes[i].enabled) {
        si[i].Index = i;
        si[i].Enabled = true;
        si[i].Identifier = BoltEditorUtils.CSharpIdentifier(Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path));
      }
    }

    foreach (var group in si.GroupBy(x => x.Identifier)) {
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
          for (int n = 0; n < si.Length; ++n) {
            if (si[n].Enabled) {
              file.EmitLine("nameLookup.Add(\"{0}\", {0});", si[n].Identifier);
              file.EmitLine("indexLookup.Add({0}, \"{0}\");", si[n].Identifier);
            }
          }
        });

        for (int n = 0; n < si.Length; ++n) {
          if (si[n].Enabled) {
            file.EmitLine("public const int {0} = {1};", si[n].Identifier, si[n].Index);
          }
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
