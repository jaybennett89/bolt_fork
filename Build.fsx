#r @"FAKE/tools/FakeLib.dll"
open Fake
open Fake.FileSystem
open Fake.FileSystemHelper
open Fake.FileUtils

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
  if (hasBuildParam "unityProjectDir") 
    then environVar "unityProjectDir"
    else "./src/bolt.unity"

let isWindows =
  System.Environment.OSVersion.Platform <> System.PlatformID.MacOSX &&
  System.Environment.OSVersion.Platform <> System.PlatformID.Unix

let unityPackageSlim =
  hasBuildParam "package-nosamples"

let unityPackageCreate = 
  hasBuildParam "package" || hasBuildParam "package-nosamples"

let unityPath = 
  if hasBuildParam "unityPath" then 
    let n = environVar "unityPath"
    log n
    log n
    log n
    log n
    n
  elif isWindows then 
    @"C:\Program Files (x86)\Unity\Editor\Unity.exe"

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
  
  mkdir (unityDir + ".samples/Assets/bolt/assemblies")
  mkdir (unityDir + ".samples/Assets/bolt/assemblies/editor")
  mkdir (unityDir + ".samples/Assets/bolt/assemblies/udpkit")

  CopyFile (unityDir + "/Assets/bolt/assemblies/") (buildDir + "/bolt.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll")
  CopyFile (unityDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll")

  CopyFile (unityDir + ".samples/Assets/bolt/assemblies/") (buildDir + "/bolt.dll")
  CopyFile (unityDir + ".samples/Assets/bolt/assemblies/editor/") (buildDir + "/bolt.editor.dll")
  CopyFile (unityDir + ".samples/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll")
)

Target "InstallBoltDebugFiles" (fun _ ->

  // have to convert pdb to mdb on windows
  if isWindows then
    let pdb2mdbPath =  @"C:\Program Files (x86)\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe";

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
  CopyFile (unityDir + "/Assets/bolt/assemblies/udpkit/") (buildDir + "/udpkit.dll.mdb")
)

Target "CreateUnityPackage" (fun _ -> 
  let dirs = 
    ["Assets/bolt/assemblies"; "Assets/bolt/resources"; "Assets/bolt/scenes"; "Assets/bolt/scripts"]
    |> String.concat " "

  execProcessCheckFail (fun s -> 
    s.FileName <- unityPath
    s.Arguments <- sprintf "-batchmode -quit -projectPath \"%s\" -executeMethod BoltUserAssemblyCompiler.Run" (directoryInfo "./src/bolt.unity").FullName
  )

  execProcessCheckFail (fun s -> 
    s.FileName <- unityPath
    s.Arguments <- sprintf "-batchmode -quit -projectPath \"%s\" -exportPackage %s \"%s/bolt.unitypackage\"" (directoryInfo "./src/bolt.unity").FullName dirs (directoryInfo buildDir).FullName
  )
)

"Clean"
  =?> ("BuildIOSNative", buildiOS)
  =?> ("BuildAndroidNative", buildAndroid)  
  ==> (if isRelease then "BuildBoltRelease" else "BuildBoltDebug")
  =?> ("InstallIOSNative", buildiOS)
  =?> ("InstallAndroidNative", buildAndroid)
  ==> "InstallBolt"
  =?> ("InstallBoltDebugFiles", not isRelease)
  =?> ("CreateUnityPackage", unityPackageCreate)

if unityPackageCreate then 
  Run "CreateUnityPackage"

elif isRelease then 
  Run "InstallBolt"

else 
  Run "InstallBoltDebugFiles"