#r @"FAKE/tools/FakeLib.dll"
#r @"FAKE/tools/FSharp.Data.dll"

open Fake
open Fake.REST

type CreateRelease =
  FSharp.Data.JsonProvider<"TypeProviderTemplates/createRelease.json">
    
let execProcessCheckFail f =
  if execProcess f (System.TimeSpan.FromMinutes 5.0) |> not then
    failwith "process failed"
    
let unityPath = 
  if hasBuildParam "unityEditorPath" then 
    environVar "unityEditorPath"

  elif not isMacOS then 
    @"C:\Program Files (x86)\Unity"

  else 
    @""
    
let hash = 
  Fake.Git.Information.getCurrentHash()

let packageUnityProject path packageName assetDirs =

  let unityExe =
    combinePaths unityPath @"Editor\Unity.exe"

  let assetDirs = 
    assetDirs
    |> List.map (fun p -> "Assets/" + p)
    |> String.concat " "

  let projectPath =
    (directoryInfo path).FullName

  execProcessCheckFail (fun s -> 
    s.FileName <- unityExe
    s.Arguments <- sprintf "-batchmode -quit -projectPath \"%s\" -exportPackage %s \"..\..\upload\%s.unitypackage\"" projectPath assetDirs packageName
  )

Target "CreatePackage" (fun _ -> 
    DeleteDir "./upload"
    CreateDir "./upload" 

    packageUnityProject "./src/bolt.unity.tutorial" (sprintf "Bolt_Beta_%s_CompleteWithTutorial" hash) ["bolt"; "bolt_tutorial"; "Gizmos"; "Plugins"]
    packageUnityProject "./src/bolt.unity.samples" (sprintf "Bolt_Beta_%s_CompleteWithSamples" hash) ["bolt"; "bolt_samples"; "Gizmos"; "Plugins"]
    packageUnityProject "./src/bolt.unity.samples" (sprintf "Bolt_Beta_%s_InstallerOnly" hash) ["bolt/assemblies/bolt.dll"; "bolt/assemblies/udpkit/udpkit.dll"; "bolt/assemblies/editor/bolt.editor.dll"]

    let zipRoot = (directoryInfo "./upload").FullName
    let zipFileName = (sprintf "./upload/Bolt_Beta_%s.zip" hash)
    let zipFiles = 
      "./upload"
      |> directoryInfo
      |> filesInDir 
      |> Seq.map (fun f -> f.FullName)

    Zip zipRoot zipFileName zipFiles
)

Target "UploadPackage" (fun _ -> 
  let ascii (s:string) = 
      System.Text.Encoding.ASCII.GetBytes(s)

  let createReq (url:string) : System.Net.WebRequest =
    let auth = (environVar "token") + ":x-oauth-basic"
    let authBase64 = System.Convert.ToBase64String(ascii auth)

    System.Console.WriteLine(auth + " / " + authBase64);

    let req = System.Net.WebRequest.CreateHttp(url)
    req.Method <- "POST"
    req.UserAgent<- "BoltEngine/bolt" 
    req.Headers.Add(System.Net.HttpRequestHeader.Authorization, ("Basic " + authBase64))
    req :> System.Net.WebRequest
        
  let createRelease_URL = "https://api.github.com/repos/BoltEngine/bolt/releases"
  let createRelease_JSON = System.String.Format("""
  {{
    "tag_name":"build_{0}",
    "target_commitish":"master",
    "name":"build_{0}",
    "body":"",
    "draft":false,
    "prerelease":true
  }}
  """, hash)
  
  let req = createReq createRelease_URL
  let data = ascii createRelease_JSON
  req.GetRequestStream().Write(data, 0, data.Length)
  
  System.Console.WriteLine(createRelease_URL);

  let res = req.GetResponse();
  let resStream = res.GetResponseStream();
  let release = CreateRelease.Load(new System.IO.StreamReader(resStream))

  let uploadAssetURL = sprintf "https://uploads.github.com/repos/BoltEngine/bolt/releases/%A/assets?name=Bolt_Beta_%s.zip" release.Id hash
  let req = createReq uploadAssetURL

  System.Console.WriteLine(uploadAssetURL);

  // additional headers
  req.ContentType <- "application/zip"
  req.Timeout <- 40 * 60 * 1000

  let data = System.IO.File.ReadAllBytes(sprintf "./upload/Bolt_Beta_%s.zip" hash);

  req.GetRequestStream().Write(data, 0, data.Length)
 
  let response = new System.IO.StreamReader(req.GetResponse().GetResponseStream());

  response.ReadToEnd() |> ignore
)

"CreatePackage" 
  ==> "UploadPackage"

Run "UploadPackage"