using Bolt.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Process = System.Diagnostics.Process;

[InitializeOnLoad]
class BoltUserAssemblyCompiler {
  static string _sourceDir;

  static string assetDir {
    get { return Application.dataPath; }
  }

  static string sourceDir {
    get { return _sourceDir; }
  }

  static string boltAssemblyPath {
    get { return BoltEditorUtilsInternal.MakePath(assetDir, "bolt", "assemblies", "bolt.dll"); }
  }

  static string boltUserAssemblyPath {
    get { return BoltEditorUtilsInternal.MakePath(assetDir, "bolt", "assemblies", "bolt.user.dll"); }
  }

  static string boltUserAssemblyAsset {
    get { return BoltEditorUtilsInternal.MakePath("Assets", "bolt", "assemblies", "bolt.user.dll"); }
  }

  static string udpkitAssemblyPath {
    get { return BoltEditorUtilsInternal.MakePath(assetDir, "bolt", "assemblies", "udpkit", "udpkit.dll"); }
  }

  static string networkFile {
    get { return BoltEditorUtilsInternal.MakePath(sourceDir, "network.cs"); }
  }

  static string projectFile {
    get { return BoltEditorUtilsInternal.MakePath(sourceDir, "project.cs"); }
  }

  static string assemblyInfoFile {
    get { return BoltEditorUtilsInternal.MakePath(sourceDir, "assemblyinfo.cs"); }
  }

  static string prefabsFile {
    get { return BoltEditorUtilsInternal.MakePath(sourceDir, "prefabs.cs"); }
  }

  static string mapsFile {
    get { return BoltEditorUtilsInternal.MakePath(sourceDir, "maps.cs"); }
  }

  static string sourceFileList {
    get {
      return
        "\"" + networkFile + "\" " +
        "\"" + prefabsFile + "\" " +
        "\"" + mapsFile + "\" " +
        "\"" + projectFile + "\" " +
        "\"" + assemblyInfoFile + "\" ";
    }
  }

  static string assemblyReferencesList {
    get {
      List<string> assemblies = new List<string>();
      assemblies.Add(unityengineAssemblyPath);
      assemblies.Add(boltAssemblyPath);
      assemblies.Add(udpkitAssemblyPath);
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
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "Frameworks/MonoBleedingEdge/lib/mono/2.0/gmcs.exe");
      }
      else {
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/lib/mono/2.0/gmcs.exe");
      }
    }
  }

  static string unityengineAssemblyPath {
    get {
      if (isOSX) {
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "Frameworks/Managed/UnityEngine.dll");
      }
      else {
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll");
      }
    }
  }

  static string monoPath {
    get {
      if (isOSX) {
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "Frameworks/MonoBleedingEdge/bin/mono");
      }
      else {
        return BoltEditorUtilsInternal.MakePath(EditorApplication.applicationContentsPath, "MonoBleedingEdge/bin/mono.exe");
      }
    }
  }

  public static ManualResetEvent Run() {
    ManualResetEvent evnt = new ManualResetEvent(false);

    try {
      // calculate source dir
      _sourceDir = BoltEditorUtilsInternal.MakePath(Path.GetDirectoryName(assetDir), "Temp", "bolt");

      // ensure temp path exists
      Directory.CreateDirectory(sourceDir);

      // setup compiler options
      BoltCompilerOperation op = new BoltCompilerOperation();
      op.projectFilePath = projectFile;
      op.project = File.Exists("Assets/bolt/project.bytes") ? File.ReadAllBytes("Assets/bolt/project.bytes").ToObject<Project>() : new Project();

      // network config
      op.networkFilePath = networkFile;
      op.assemblyInfoFilePath = assemblyInfoFile;

      // maps config
      op.scenesFilePath = mapsFile;
      op.prefabsFilePath = prefabsFile;

      // run code emitter
      BoltCompiler.Run(op);
      RunCSharpCompiler(op, evnt);

    }
    catch (Exception exn) {
      evnt.Set();
      Debug.LogException(exn);
    }

    return evnt;
  }

  static void RunCSharpCompiler(BoltCompilerOperation op, ManualResetEvent evnt) {
#if DEBUG
    const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug+ -optimize- -warn:{3} ";
#else
    const string CMD_ARGS = "\"{0}\" -out:\"{1}\" {2} -platform:anycpu -target:library -debug- -optimize+ -warn:{3} ";
#endif

    string args = CMD_ARGS;

    if (BoltEditorUtilsInternal.hasPro == false) {
      args += "-define:UNITY_NOT_PRO ";
    }

    if (BoltCore.isDebugMode) {
      args += "-define:DEBUG ";
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
      // we are done
      evnt.Set();

      // continue
      BoltEditor.Internal.EditorHousekeeping.Invoke(() => {
        if (p.ExitCode == 0) { CompilationDone(op); }
      });
    };

    p.Start();
    p.BeginErrorReadLine();
    p.BeginOutputReadLine();
  }

  static void CompilationDone(BoltCompilerOperation op) {
    AssetDatabase.ImportAsset(boltUserAssemblyAsset, ImportAssetOptions.ForceUpdate);

    Debug.Log("BoltCompiler: Success!");

    EditorPrefs.SetInt("BOLT_UNCOMPILED_COUNT", 0);
    EditorPrefs.SetBool(BoltScenesWindow.COMPILE_SETTING, false);
  }

  static void OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
    if (e.Data != null) {
      Debug.Log(e.Data);
    }
  }

  static void ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e) {
    if (e.Data != null) {
      if (e.Data.Contains(": warning") && !e.Data.Contains(": error")) {
        Debug.LogWarning(e.Data);
      }
      else {
        Debug.LogError(e.Data);
      }
    }
  }
}
