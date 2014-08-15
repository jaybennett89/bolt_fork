#r @"FAKE/tools/FakeLib.dll"
open Fake
open Fake.FileSystem
open Fake.FileSystemHelper
open Fake.FileUtils


let ndkPath = @"C:/android_ndk/ndk-build.cmd"
let iosDir = "./src/bolt/udpkit.native/ios"
let androidDir = "./src/bolt/udpkit.native/android"
let unityDir = "./src/bolt.unity"
let buildDir = "./build"
let buildDirUdpKit = "./build/udpkit"
let rootDir = currentDirectory
let isWindows =
  System.Environment.OSVersion.Platform <> System.PlatformID.MacOSX &&
  System.Environment.OSVersion.Platform <> System.PlatformID.Unix

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

//Target "BuildUnityPackage" (fun _ ->
//  execProcessCheckFail (fun s ->
//    s.FileName <- @"C:\Program Files (x86)\Unity\Editor\Unity.exe"
//    s.Arguments <- (sprintf "-quit -batchmode -projectPath \"%s\" -exportPackage Assets/Tutorial/TutorialAssets \"%s/TutorialAssets.unitypackage\"" (directoryInfo unityDir).FullName (directoryInfo buildDir).FullName)
//  )
//  
//  CopyFile "./src/bolt/bolt.editor/Resources" "./build/TutorialAssets.unitypackage" 
//)

let boltProjects = [
  "./src/bolt/bolt.sln"
]

Target "BuildBolt" (fun _ ->
  boltProjects
  |> MSBuildDebug buildDir "Build"
  |> Log "AppBuild-Output: "
)

Target "InstallAndroidNative" (fun _ ->
  //mkdir "./src/bolt.unity/Assets/Plugins/Android"
  //CopyFile "./src/bolt.unity/Assets/Plugins/Android" (buildDir + "/libudpkit_android.so")
  ()
)

Target "InstallIOSNative" (fun _ ->
  //mkdir "./src/bolt.unity/Assets/Plugins/iOS"
  //CopyFile "./src/bolt.unity/Assets/Plugins/iOS" (buildDir + "/libudpkit_ios.a")
  ()
)

Target "InstallBolt" (fun _ ->
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies" (buildDir + "/bolt.dll")
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies/editor/" (buildDir + "/bolt.editor.dll")
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies/udpkit/" (buildDir + "/udpkit.dll")
)

Target "InstallBoltDebugFiles" (fun _ ->
  let pdb2mdbPath = @"C:\Program Files (x86)\Unity\Editor\Data\MonoBleedingEdge\lib\mono\4.0\pdb2mdb.exe";

  if isWindows then
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
    
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies" (buildDir + "/bolt.dll.mdb")
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies/editor/" (buildDir + "/bolt.editor.dll.mdb")
  CopyFile "./src/bolt.unity/Assets/bolt/assemblies/udpkit/" (buildDir + "/udpkit.dll.mdb")
)

"Clean"
  =?> ("BuildIOSNative", not isWindows)
  =?> ("BuildAndroidNative", isWindows)  
  ==> "BuildBolt" 
  =?> ("InstallIOSNative", not isWindows)
  =?> ("InstallAndroidNative", isWindows)
  ==> "InstallBolt"
  ==> "InstallBoltDebugFiles"

Run "InstallBoltDebugFiles"