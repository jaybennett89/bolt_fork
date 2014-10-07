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
let isRelease = hasBuildParam "release"
let buildiOS = hasBuildParam "ios" && isMacOS
let buildAndroid = hasBuildParam "ndkbuild"

let iosDir = "./src/bolt/udpkit.native/ios"
let androidDir = "./src/bolt/udpkit.native/android"
let buildDir = "./build"
let buildDirUdpKit = "./build/udpkit"
let rootDir = currentDirectory

let unityDir =
  if (hasBuildParam "project")
    then environVar "project"
    else "./src/bolt.unity"

let unityPath = 
  if hasBuildParam "unity" then 
    environVar "unity"

  elif not isMacOS then 
    @"C:\Program Files (x86)\Unity"

  else 
    @""

let execProcessCheckFail f =
  if execProcess f (System.TimeSpan.FromMinutes 5.0) |> not then
    failwith "process failed"

Target "Clean" (fun _ ->
  CleanDirs [buildDir]
)

Target "BuildIOSNative" (fun _ ->
  cd iosDir
  execProcessCheckFail (fun s -> 
    s.FileName <- "xcodebuild"
    s.Arguments <- "ARCHS=\"armv7 armv7s\" ONLY_ACTIVE_ARCH=NO -target udpkit_ios -target udpkit_ios -scheme udpkit_ios -derivedDataPath ./BuildOutput"
  )
  cd rootDir
  CopyFile "./build" (iosDir + "/BuildOutput/Build/Products/Debug-iphoneos/libudpkit_ios.a")
  CopyFile "./src/bolt/bolt.editor/Resources" "./build/libudpkit_ios.a" 
)

Target "BuildAndroidNative" (fun _ ->
  cd androidDir
  execProcessCheckFail (fun s -> 
    s.FileName <- ndkPath
    s.Arguments <- "V=1"
  )
  cd rootDir
  CopyFile "./build" (androidDir + "/libs/armeabi/libudpkit_android.so")
  CopyFile "./src/bolt/bolt.editor/Resources" "./build/libudpkit_android.so" 
)

Target "BuildBoltDebug" (fun _ ->
  ["./src/bolt/bolt.sln"]
  |> MSBuildDebug buildDir "Build"
  |> Log "AppBuild-Output: "
)

Target "BuildBoltRelease" (fun _ ->
  ["./src/bolt/bolt.sln"]
  |> MSBuildRelease buildDir "Build"
  |> Log "AppBuild-Output: "
)

Target "InstallAndroidNative" (fun _ ->
  mkdir "./src/bolt.unity/Assets/Plugins/Android"
  CopyFile (unityDir + "/Assets/Plugins/Android") (buildDir + "/libudpkit_android.so")
)

Target "InstallIOSNative" (fun _ ->
  mkdir "./src/bolt.unity/Assets/Plugins/iOS"
  CopyFile (unityDir + "/Assets/Plugins/iOS") (buildDir + "/libudpkit_ios.a")
)

Target "InstallBolt" (fun _ ->
  mkdir (unityDir + "/Assets/bolt/assemblies")
  mkdir (unityDir + "/Assets/bolt/assemblies/editor")
  mkdir (unityDir + "/Assets/bolt/assemblies/udpkit")

  CopyFile (unityDir + "/Assets/bolt/assemblies/") (buildDir + "/bolt.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.compiler.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/protobuf-net.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll")
)

Target "InstallBoltDebugFiles" (fun _ ->
  if not isMacOS then
    let pdb2mdbPath = unityPath + @"\Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe";

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
  CopyFile (unityDir + "/Assets/bolt/assemblies/") (buildDir + "/bolt.dll.mdb")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll.mdb")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.compiler.dll.mdb")
  CopyFile (unityDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll.mdb")
)

"Clean"
  =?> ("BuildIOSNative", buildiOS)
  =?> ("BuildAndroidNative", buildAndroid)  
  ==> (if isRelease then "BuildBoltRelease" else "BuildBoltDebug")
  =?> ("InstallIOSNative", buildiOS)
  =?> ("InstallAndroidNative", buildAndroid)
  ==> "InstallBolt"
  =?> ("InstallBoltDebugFiles", not isRelease)

if isRelease 
  then Run "InstallBolt"
  else Run "InstallBoltDebugFiles"