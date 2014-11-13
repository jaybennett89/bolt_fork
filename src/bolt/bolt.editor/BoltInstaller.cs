using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UE = UnityEngine;

[InitializeOnLoad]
public static class BoltInstaller {
  const string INSTALLED_VERSION_KEY = "BOLT_INSTALLED_VERSION";

  static Assembly Assembly {
    get { return Assembly.GetExecutingAssembly(); }
  }

  static string CurrentVersion {
    get { return Assembly.GetName().Version.ToString(); }
  }

  static string InstalledVersion {
    get {
      if (!EditorPrefs.HasKey(INSTALLED_VERSION_KEY)) {
        return "None/Unknown";
      }

      return EditorPrefs.GetString(INSTALLED_VERSION_KEY);
    }
  }

  static string[] Resources {
    get {
      return Assembly.GetManifestResourceNames();
    }
  }

  static ImportAssetOptions ImportOptions {
    get {
      return ImportAssetOptions.Default;
    }
  }

  public static void Run() {
    string error = null;

    if (BoltEditorUtilsInternal.isEditorPlaying) {
      error = "You can't install Bolt when the editor is playing";
    }
    else if (EditorApplication.isCompiling) {
      error = "You can't install Bolt when the editor is compiling scripts";
    }
    else if (EditorApplication.isUpdating) {
      error = "You can't install Bolt when the editor is updating the asset database";
    }

    if (error != null) {
      EditorUtility.DisplayDialog("Bolt Installer", error, "Ok");
      return;
    }

    Debug.Log(string.Format("Installing Bolt v{0}", CurrentVersion));

    try {
      EditorApplication.LockReloadAssemblies();

      EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "resources"));

      InstallLogo();
      InstallIcons();
      InstallDocumentation();
      InstallGizmos();
      InstallRuntimeSettings();
      CompileUserAssembly();
      InstallScripts();
      InstallAreaOfInterest();
      CreateDebugScene();
      InstallPlugins();

      // update
      EditorPrefs.SetString(INSTALLED_VERSION_KEY, CurrentVersion);
    }
    finally {
      EditorApplication.UnlockReloadAssemblies();
      EditorUtility.ClearProgressBar();
    }
  }

  static void InstallAreaOfInterest() {
    Progress("Installing area of interest assets ... ", 0f);

    var shader = Resources.First(x => x.Contains("BoltShaderPOI.shader"));
    InstallAsset(shader);

    var mesh = Resources.First(x => x.Contains("IcoSphere.fbx"));
    InstallAsset(mesh);

    EditImporter<ModelImporter>(ResourceToAssetPath(mesh), m => {
      m.globalScale = 1f;
      m.importMaterials = false;
    });

    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    Progress("Installing area of interest assets ... ", 1f);
  }

  static void InstallDocumentation() {
    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "documentation"));

    Progress("Installing documentation ... ", 0f);

    foreach (var resource in Resources.Where(x => x.Contains("Install.bolt.documentation"))) {
      InstallAsset(resource);
    }

    Progress("Installing documentation ... ", 1f);
  }

  static void InstallLogo() {
    Progress("Installing logo ... ", 0f);

    var resourceLogo = Resources.First(x => x.Contains("Install.bolt.resources.BoltLogo"));
    var resourceIcon = Resources.First(x => x.Contains("Install.bolt.resources.BoltIcon"));

    // install asset
    InstallAsset(resourceLogo);
    InstallAsset(resourceIcon);

    // edit asset
    EditImporter<TextureImporter>(ResourceToAssetPath(resourceLogo), txt => {
      txt.textureFormat = TextureImporterFormat.ARGB32;
      txt.wrapMode = TextureWrapMode.Clamp;
      txt.filterMode = FilterMode.Bilinear;
      txt.alphaIsTransparency = true;
      txt.maxTextureSize = 256;
    });

    EditImporter<TextureImporter>(ResourceToAssetPath(resourceIcon), txt => {
      txt.textureFormat = TextureImporterFormat.ARGB32;
      txt.wrapMode = TextureWrapMode.Clamp;
      txt.filterMode = FilterMode.Bilinear;
      txt.alphaIsTransparency = true;
      txt.maxTextureSize = 256;
    });

    Progress("Installing logo ... ", 1f);
  }

  static void InstallIcons() {
    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "resources", "icons"));
    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "resources", "backgrounds"));

    var icons = 
      Resources
        .Where(x => x.Contains("Install.bolt.resources.icons"))
        .Concat(Resources.Where(x => x.Contains("Install.bolt.resources.backgrounds")))
        .ToArray();

    for (int i = 0; i < icons.Length; ++i) {
      Progress("Installing editor icons ... ", i, icons.Length);

      // install asset
      InstallAsset(icons[i]);

      if (icons[i].EndsWith(".png")) {
        // edit asset
        EditImporter<TextureImporter>(ResourceToAssetPath(icons[i]), txt => {
          txt.textureFormat = TextureImporterFormat.ARGB32;
          txt.wrapMode = TextureWrapMode.Clamp;
          txt.filterMode = FilterMode.Bilinear;
          txt.textureType = TextureImporterType.GUI;
          txt.alphaIsTransparency = true;
          txt.maxTextureSize = 32;
        });
      }
    }
  }

  static void InstallGizmos() {
    Progress("Installing gizmos ... ", 0f);

    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "Gizmos"));

    var gizmos = Resources.Where(x => x.Contains("Install.Gizmos")).ToArray();

    for (int i = 0; i < gizmos.Length; ++i) {
      Progress("Installing gizmos ... ", i, gizmos.Length);

      // install asset
      InstallAsset(gizmos[i]);

      // edit asset
      EditImporter<TextureImporter>(ResourceToAssetPath(gizmos[i]), txt => {
        txt.textureFormat = TextureImporterFormat.ARGB32;
        txt.wrapMode = TextureWrapMode.Clamp;
        txt.filterMode = FilterMode.Point;
        txt.textureType = TextureImporterType.GUI;
        txt.alphaIsTransparency = true;
        txt.maxTextureSize = 64;
      });
    }
  }

  static void InstallPlugins() {
    Progress("Installing native plugins ... ", 0f);

    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "Plugins"));
    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "Plugins", "iOS"));
    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "Plugins", "Android"));

    var plugins = Resources.Where(x => x.Contains("Install.Plugins")).ToArray();

    for (int i = 0; i < plugins.Length; ++i) {
      Progress("Installing native plugins  ... ", i, plugins.Length);

      // install asset
      InstallAsset(plugins[i]);
    }
  }

  static void InstallRuntimeSettings() {
    Progress("Creating settings resource ... ", 0f);

    const string SETTINGS_PATH = "Assets/bolt/resources/BoltRuntimeSettings.asset";
    const string PREFABDB_PATH = "Assets/bolt/resources/BoltPrefabDatabase.asset";

    if (!AssetDatabase.LoadAssetAtPath(SETTINGS_PATH, typeof(BoltRuntimeSettings))) {
      AssetDatabase.CreateAsset(BoltRuntimeSettings.CreateInstance<BoltRuntimeSettings>(), SETTINGS_PATH);
      AssetDatabase.ImportAsset(SETTINGS_PATH, ImportOptions);
    }

    if (!AssetDatabase.LoadAssetAtPath(PREFABDB_PATH, typeof(Bolt.PrefabDatabase))) {
      AssetDatabase.CreateAsset(Bolt.PrefabDatabase.CreateInstance<Bolt.PrefabDatabase>(), PREFABDB_PATH);
      AssetDatabase.ImportAsset(PREFABDB_PATH, ImportOptions);
    }

    Progress("Creating settings resource ... ", 1f);
  }

  static void CompileUserAssembly() {
    Progress("Compiling user assembly ... ", 0f);

    BoltUserAssemblyCompiler.Run().WaitOne();

    Progress("Compiling user assembly ... ", 1f);
  }

  static void EnsureDirectoryExists(string directory) {
    if (!Directory.Exists(directory)) {
      Directory.CreateDirectory(directory);
    }
  }

  static void InstallScripts() {
    Progress("Installing scripts ... ", 0);

    var scripts = Resources.Where(x => x.Contains("Install.bolt.scripts")).ToArray();
    var scriptsPath = BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "scripts");

    EnsureDirectoryExists(scriptsPath);

    foreach (var f in Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories)) {
      CleanExistingFile(f);
    }

    for (int i = 0; i < scripts.Length; ++i) {
      Progress("Installing scripts ... ", i, scripts.Length);

      var data = BoltEditorUtilsInternal.GetResourceBytes(scripts[i]);
      var file = ResourceToFilePath(scripts[i]);
      var asset = ResourceToAssetPath(scripts[i]);

      EnsureDirectoryExists(Path.GetDirectoryName(file));

      // write file
      File.WriteAllBytes(file, data);
    }

    // import all script assets
    for (int i = 0; i < scripts.Length; ++i) {
      AssetDatabase.ImportAsset(ResourceToAssetPath(scripts[i]), ImportAssetOptions.ForceUpdate);
    }

    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
  }

  static void CreateDebugScene() {
    Progress("Creating debug start scene ... ", 0);

    EnsureDirectoryExists(BoltEditorUtilsInternal.MakePath(Application.dataPath, "bolt", "scenes"));

    // create new scene
    EditorApplication.NewScene();

    // add debug script thingy
    var debugStartScript = (MonoScript)AssetDatabase.LoadAssetAtPath(BoltEditorUtilsInternal.MakeAssetPath("bolt", "scripts", "BoltDebugStart.cs"), typeof(MonoScript));
    InternalEditorUtility.AddScriptComponentUnchecked(new GameObject("BoltDebugStart"), debugStartScript);

    // save scene
    EditorApplication.SaveScene(BoltEditorUtilsInternal.MakeAssetPath("bolt/scenes/BoltDebugScene.unity"));

    // create a new scene
    EditorApplication.NewScene();

    Progress("Creating debug start scene ... ", 1);
  }

  static void InstallAsset(string resource) {
    var data = BoltEditorUtilsInternal.GetResourceBytes(resource);
    var file = ResourceToFilePath(resource);
    var asset = ResourceToAssetPath(resource);

    Debug.Log("installing:" + file);

    // delete existing asset
    // AssetDatabase.DeleteAsset(asset);

    // write new file
    File.WriteAllBytes(file, data);

    // import new asset
    AssetDatabase.ImportAsset(asset, ImportOptions);
  }

  static void EditImporter<T>(string assetPath, Action<T> edit) where T : AssetImporter {
    T importer;

    importer = (T)AssetImporter.GetAtPath(assetPath);

    edit(importer);

    AssetDatabase.ImportAsset(assetPath, ImportOptions);
  }

  static void EditAsset<T>(string assetPath, Action<T> edit) where T : UE.Object {
    T asset;

    asset = (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));

    edit(asset);

    AssetDatabase.SaveAssets();
  }

  static void Progress(string info, float progress) {
    EditorUtility.DisplayProgressBar("Installing Bolt", info, progress);
  }

  static void Progress(string info, int current, int max) {
    Progress(info, (float)current / (float)(max - 1));
  }

  static string ResourceToPath(string resource) {
    resource = resource.Replace("bolt.editor.Resources.Install.", "");
    resource =
        resource.Substring(0, resource.LastIndexOf('.')).Replace('.', '/')
        +
        resource.Substring(resource.LastIndexOf('.'));

    return resource;
  }

  static string ResourceToFilePath(string resource) {
    return BoltEditorUtilsInternal.MakePath(Application.dataPath, ResourceToPath(resource));
  }

  static string ResourceToAssetPath(string resource) {
    return BoltEditorUtilsInternal.MakeAssetPath(ResourceToPath(resource)).Replace('\\', '/');
  }

  static void CleanExistingFile(string file) {
    if (File.Exists(file)) {
      File.Delete(file);
    }
  }
}

