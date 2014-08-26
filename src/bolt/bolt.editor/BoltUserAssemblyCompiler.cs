using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Process = System.Diagnostics.Process;

[InitializeOnLoad]
class BoltUserAssemblyCompiler {

  static string _sourceDir;

  static string ds {
    get { return Path.DirectorySeparatorChar.ToString(); }
  }

  static string assetDir {
    get { return Application.dataPath; }
  }

  static string sourceDir {
    get { return _sourceDir; }
  }

  static string boltAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "bolt.dll"); }
  }

  static string boltUserAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "bolt.user.dll"); }
  }

  static string boltUserAssemblyAsset {
    get { return BoltEditorUtils.MakePath("Assets", "bolt", "assemblies", "bolt.user.dll"); }
  }

  static string udpkitAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "udpkit", "udpkit.dll"); }
  }

  static string udpkitAndroidAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "udpkit", "udpkit.platform.android.dll"); }
  }

  static string udpkitIOSAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "udpkit", "udpkit.platform.ios.dll"); }
  }

  static string udpkitManagedAssemblyPath {
    get { return BoltEditorUtils.MakePath(assetDir, "bolt", "assemblies", "udpkit", "udpkit.platform.managed.dll"); }
  }

  static string eventsFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "events.cs"); }
  }

  static string statesFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "states.cs"); }
  }

  static string networkFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "network.cs"); }
  }

  static string assemblyInfoFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "assemblyinfo.cs"); }
  }

  static string prefabsFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "prefabs.cs"); }
  }

  static string mecanimFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "mecanim.cs"); }
  }

  static string mapsFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "maps.cs"); }
  }

  static string commandsFile {
    get { return BoltEditorUtils.MakePath(sourceDir, "commands.cs"); }
  }

  static string sourceFileList {
    get {
      return
        "\"" + eventsFile + "\" " +
        "\"" + networkFile + "\" " +
        "\"" + statesFile + "\" " +
        "\"" + prefabsFile + "\" " +
        "\"" + mapsFile + "\" " +
        "\"" + mecanimFile + "\" " +
        "\"" + commandsFile + "\" " +
        "\"" + assemblyInfoFile + "\" ";
    }
  }

  static string assemblyReferencesList {
    get {
      List<string> assemblies = new List<string>();
      assemblies.Add(unityengineAssemblyPath);
      assemblies.Add(boltAssemblyPath);
      assemblies.Add(udpkitAssemblyPath);

      if (File.Exists(udpkitAndroidAssemblyPath)) assemblies.Add(udpkitAndroidAssemblyPath);
      if (File.Exists(udpkitIOSAssemblyPath)) assemblies.Add(udpkitIOSAssemblyPath);
      if (File.Exists(udpkitManagedAssemblyPath)) assemblies.Add(udpkitManagedAssemblyPath);

      return string.Join(" ", assemblies.Select(x => "-reference:\"" + x + "\"").ToArray());
    }
  }

  static bool isOSX {
    get { return !isWIN; }
  }

  static bool isWIN {
    get {
      return
        Environment.OSVersion.Platform == PlatformID.Win32NT ||
        Environment.OSVersion.Platform == PlatformID.Win32S ||
        Environment.OSVersion.Platform == PlatformID.Win32Windows ||
        Environment.OSVersion.Platform == PlatformID.WinCE;
    }
  }

  static string csharpCompilerPath {
    get {
      if (isOSX) {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "Frameworks/MonoBleedingEdge/lib/mono/2.0/gmcs.exe");
      } else {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono/2.0/gmcs.exe");
      }
    }
  }

  static string unityengineAssemblyPath {
    get {
      if (isOSX) {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEngine.dll");
      } else {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
      }
    }
  }

  static string monoPath {
    get {
      if (isOSX) {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "Frameworks/MonoBleedingEdge/bin/mono");
      } else {
        return BoltEditorUtils.MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/mono.exe");
      }
    }
  }

  static IEnumerable<BoltPrefab> FindPrefabs () {
    int id = 0;

    foreach (var file in Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories)) {
      BoltEntity entity = AssetDatabase.LoadAssetAtPath(file, typeof(BoltEntity)) as BoltEntity;

      if (entity) {
        if (entity) {
          entity.SetField("_prefabId", id);
          entity._sceneObject = false;

          EditorUtility.SetDirty(entity.gameObject);
          EditorUtility.SetDirty(entity);

          yield return new BoltPrefab { go = entity.gameObject, id = id, name = entity.gameObject.name.CSharpIdentifier() };

          id += 1;

        } else {
          entity = null;
          EditorUtility.UnloadUnusedAssets();
        }
      }
    }
  }

  public static void Run () {
    try {
      if (EditorApplication.isCompiling) {
        Debug.LogError("Can't compile Bolt while unity is compiling it's own resources.");
        return;
      }

#if DEBUG
      // clear developer console
      Debug.ClearDeveloperConsole();
#endif

      // calculate source dir
      _sourceDir = BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir), "Temp", "bolt");

      // ensure temp path exists
      Directory.CreateDirectory(sourceDir);

      // setup compiler options
      BoltCompilerOperation op = new BoltCompilerOperation();

      // network config
      op.networkFilePath = networkFile;
      op.assemblyInfoFilePath = assemblyInfoFile;

      // events config
      op.eventIdOffset = BoltEventBase.USER_START_ID;
      op.eventFilePath = eventsFile;
      op.events = BoltEditorUtils.FindAssets<BoltEventAsset>().ToList();

      // commands config
      op.commands = BoltEditorUtils.FindAssets<BoltCommandAsset>().ToList();
      op.commandsFilePath = commandsFile;

      // maps config
      op.mapsFilePath = mapsFile;

      // prefabs config
      op.prefabsFilePath = prefabsFile;
      op.prefabs = FindPrefabs().ToList();

      // mecanim config
      op.mecanimFilePath = mecanimFile;
      op.mecanims = BoltEditorUtils.FindAssets<BoltMecanimAsset>().ToList();

      // state config
      op.stateFilePath = statesFile;
      op.states = BoltEditorUtils.FindAssets<BoltStateAsset>().ToList();

      // run code emitter
      BoltCompiler.Run(op);
      RunCSharpCompiler(op);

    } catch (Exception exn) {
      Debug.LogException(exn);
    }
  }

  static void ImportCSharpFiles () {
    AssetDatabase.ImportAsset(eventsFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(networkFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(commandsFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(mecanimFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(mapsFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(statesFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
    AssetDatabase.ImportAsset(prefabsFile.Replace(BoltEditorUtils.MakePath(Path.GetDirectoryName(assetDir)), "").Replace('\\', '/').Trim('/'), ImportAssetOptions.ForceUpdate);
  }

  static void RunCSharpCompiler (BoltCompilerOperation op) {
#if DEBUG
    const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug+ -optimize- -warn:{3} ";
#else
    const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug- -optimize+ -warn:{3} ";
#endif

    string args = CMD_ARGS;

    if (BoltEditorUtils.hasPro == false) {
      args += "-define:UNITY_NOT_PRO ";
    }

    Process p = new Process();
    p.StartInfo.FileName = monoPath;
    p.StartInfo.Arguments = string.Format(args + sourceFileList, csharpCompilerPath, boltUserAssemblyPath, assemblyReferencesList, Mathf.Clamp(BoltRuntimeSettings.instance.compilationWarnLevel, 0, 4));

    p.EnableRaisingEvents = true;
    p.StartInfo.CreateNoWindow = true;
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardError = true;
    p.StartInfo.RedirectStandardOutput = true;

    p.ErrorDataReceived += ErrorDataReceived;
    p.OutputDataReceived += OutputDataReceived;

    p.Exited += (s, ea) => {
      BoltMainThreadInvoker.Invoke(() => {
        if (p.ExitCode == 0) { CompilationDone(op); }
      });
    };

    p.Start();
    p.BeginErrorReadLine();
    p.BeginOutputReadLine();
  }

  static void CompilationDone (BoltCompilerOperation op) {
    AssetDatabase.ImportAsset(boltUserAssemblyAsset, ImportAssetOptions.ForceUpdate);

    UpdateUserAssemblyHash();

    ClearCompileFlag(op.events);
    ClearCompileFlag(op.states);
    ClearCompileFlag(op.mecanims);
    ClearCompileFlag(op.commands);

    Debug.Log("BoltCompiler: Success!");

    EditorPrefs.SetInt("BOLT_UNCOMPILED_COUNT", 0);
    EditorPrefs.SetBool(BoltScenesWindow.COMPILE_SETTING, false);
  }

  static void UpdateUserAssemblyHash () {
    byte[] assembly = File.ReadAllBytes(boltUserAssemblyPath);
    System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
    byte[] hashBytes = md5.ComputeHash(assembly);

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("public static class BoltUserAssemblyHash {");
    sb.Append("  public static readonly byte[] value = new byte[] { ");

    for (int i = 0; i < hashBytes.Length; ++i) {
      sb.AppendFormat("0x{0:x2}, ", hashBytes[i]);
    }

    sb.Append("};");
    sb.AppendLine();
    sb.AppendLine("}");

    File.WriteAllText("Assets/bolt/scripts/BoltUserAssemblyHash.cs", sb.ToString());
    AssetDatabase.ImportAsset("Assets/bolt/scripts/BoltUserAssemblyHash.cs", ImportAssetOptions.ForceUpdate);
  }

  static void ClearCompileFlag<T> (IEnumerable<T> assets) where T : BoltCompilableAsset {
    foreach (T asset in assets) {
      asset.compile = false;
      EditorUtility.SetDirty(asset);
    }
  }

  static void OutputDataReceived (object sender, System.Diagnostics.DataReceivedEventArgs e) {
    if (e.Data != null) {
      Debug.Log(e.Data);
    }
  }

  static void ErrorDataReceived (object sender, System.Diagnostics.DataReceivedEventArgs e) {
    if (e.Data != null) {
      if (e.Data.Contains(": warning") && !e.Data.Contains(": error")) {
        Debug.LogWarning(e.Data);
      } else {
        Debug.LogError(e.Data);
      }
    }
  }

  static Type FindCommandType () {
    foreach (Type t in typeof(BoltCommand).FindSubtypes()) {
      if (t.HasPublicDefaultConstructor()) {
        return t;
      }
    }

    return null;
  }
}
