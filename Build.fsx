#r @"FAKE/tools/FakeLib.dll"

open Fake
open Fake.FileSystem
open Fake.FileSystemHelper
open Fake.FileUtils

let rec findDirs (skip:string list) dir = 
  seq {
    yield dir

    for sub in subDirectories dir do
      if skip |> List.exists (fun d -> sub.Name.ToLowerInvariant() = d.ToLowerInvariant()) |> not then
        yield! findDirs skip sub
  }

let rec findFiles pattern dirs =
  seq {
    for d in dirs do
      for f in filesInDirMatching pattern d do
        yield f
  }

let ndkPath = environVar "ndkbuild"
let configuration = getBuildParam "configuration"
let isdebug = configuration.Contains("Debug")

let rootDir = currentDirectory
let projectDir = getBuildParamOrDefault "project" ""
let unityDir = getBuildParamOrDefault "unity" @"C:\Program Files (x86)\Unity"
let buildDir = rootDir + "\\build"

let execProcessCheckFail f =
  if execProcess f (System.TimeSpan.FromMinutes 5.0) |> not then
    failwith "process failed"

Target "Clean" (fun _ ->
  CleanDirs [buildDir]
)

Target "Build" (fun _ -> 
  build (fun d ->
    { d with
        Verbosity = Some(Quiet)
        Targets = ["Build"]
        Properties = 
        [
          "Configuration", configuration
          "OutputPath", buildDir
          "UnityPathWin", unityDir
          "UnityPathOSX", unityDir
        ]
    }
  ) "src\\bolt.sln"
)

Target "Install" (fun _ ->
  if projectDir <> "" then
    mkdir (projectDir + "/Assets/bolt/assemblies")
    mkdir (projectDir + "/Assets/bolt/assemblies/upnp")
    mkdir (projectDir + "/Assets/bolt/assemblies/editor")
    mkdir (projectDir + "/Assets/bolt/assemblies/udpkit")
    
    CopyFile (projectDir + "/Assets/bolt/assemblies/") (buildDir + "/bolt.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.compiler.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/protobuf-net.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") ("./src/assemblies/protobuf-net.LICENSE.txt")
    CopyFile (projectDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.common.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/upnp/Mono.Nat.bytes") ("./src/assemblies/Mono.Nat.dll")
    CopyFile (projectDir + "/Assets/bolt/assemblies/upnp/") ("./src/assemblies/Mono.Nat.LICENSE.txt")
)

Target "InstallDebug" (fun _ ->
  if configuration = "Debug" then
    if not isMacOS then
      let pdb2mdbPath = unityDir + @"\Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe";

      (directoryInfo "./build")
      |> filesInDirMatching "*.pdb"
      |> Seq.iter (fun file ->
        {
          ExecParams.Program = pdb2mdbPath
          ExecParams.CommandLine = @" """ + file.Name.Replace(".pdb", ".dll") + @""""
          ExecParams.Args = List.empty
          ExecParams.WorkingDirectory = (directoryInfo "./build").FullName
        }
        |> shellExec
        |> ignore
      )
    
    // copy files into unity folder
    CopyFile (projectDir + "/Assets/bolt/assemblies/") (buildDir + "/bolt.dll.mdb")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll.mdb")
    CopyFile (projectDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.compiler.dll.mdb")
    CopyFile (projectDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll.mdb")
    CopyFile (projectDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.common.dll.mdb")
)

("Clean")
  ==> ("Build")
  ==> ("Install")
  =?> ("InstallDebug", configuration = "Debug")
  
Run "Install"
