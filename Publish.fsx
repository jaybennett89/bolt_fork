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
    
let buildName =
  let now = System.DateTime.Now
  sprintf "Bolt_Daily_Y%iM%iD%i" now.Year now.Month now.Day

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

    packageUnityProject "./src/bolt.unity.tutorial" (sprintf "%s_CompleteWithTutorial" buildName) ["bolt"; "bolt_tutorial"; "Gizmos"; "Plugins"]
    packageUnityProject "./src/bolt.unity.samples" (sprintf "%s_CompleteWithSamples" buildName) ["bolt"; "bolt_samples"; "Gizmos"; "Plugins"]
    packageUnityProject "./src/bolt.unity.samples" (sprintf "%s_InstallerOnly" buildName) ["bolt/assemblies/bolt.dll"; "bolt/assemblies/udpkit/udpkit.dll"; "bolt/assemblies/editor/bolt.editor.dll"]

    let zipRoot = (directoryInfo "./upload").FullName
    let zipFileName = (sprintf "./upload/%s.zip" buildName)
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

  let authToken =
    let auth = (environVar "token") + ":x-oauth-basic"
    "Basic " + System.Convert.ToBase64String(ascii auth)


  let postFile2 (url:string) (data:byte array) =
    let wc = new System.Net.WebClient();
    let re = new System.Threading.ManualResetEvent(false)

    wc.Headers.Add("User-Agent",  "BoltEngine/bolt")
    wc.Headers.Add("Content-Length", data.Length.ToString())
    wc.Headers.Add("Content-Type", "application/zip")
    wc.Headers.Add("Authorization", authToken)

    let progress = ref 0L

    wc.UploadDataCompleted.Add (fun args -> re.Set() |> ignore)
    wc.UploadProgressChanged.Add (fun args -> 
      System.Console.WriteLine("PROGRESS CHANGED")
      progress := args.BytesSent
    )

    wc.UploadDataAsync(new System.Uri(url), data)

    while re.WaitOne(1000) do
      System.Console.WriteLine(sprintf "Progress %i / %i" !progress data.Length)

  let postFile (url:string) (data:byte array) =
    System.Console.WriteLine(sprintf "UPLOADING FILE (%i Bytes)" data.Length)
    System.Console.WriteLine(url)

    let req = System.Net.WebRequest.CreateHttp(url)
    req.Method <- "POST"
    req.UserAgent <- "BoltEngine/bolt" 
    req.ContentLength <- int64 data.Length
    req.ContentType <- "application/zip"
    req.Timeout <- 40 * 60 * 1000
    req.Headers.Add(System.Net.HttpRequestHeader.Authorization, authToken)
    
    let stream = req.GetRequestStream()

    stream.Write(data, 0, data.Length)
    stream.Flush()

    let response = new System.IO.StreamReader(req.GetResponse().GetResponseStream())

    response.ReadToEnd() |> ignore

  let createReq (url:string) : System.Net.WebRequest =
    let auth = (environVar "token") + ":x-oauth-basic"
    let authBase64 = System.Convert.ToBase64String(ascii auth)

    System.Console.WriteLine(auth + " / " + authBase64);

    let req = System.Net.WebRequest.CreateHttp(url)
    req.Method <- "POST"
    req.UserAgent<- "BoltEngine/bolt" 
    req.Headers.Add(System.Net.HttpRequestHeader.Authorization, authToken)
    req :> System.Net.WebRequest

  let createRelease_URL = "https://api.github.com/repos/BoltEngine/bolt/releases"
  let createRelease_JSON = System.String.Format("""
  {{
    "tag_name":"{0}",
    "target_commitish":"master",
    "name":"{0}",
    "body":"{0}",
    "draft":false,
    "prerelease":true
  }}
  """, buildName)
  
  let req = createReq createRelease_URL
  let data = ascii createRelease_JSON
  req.GetRequestStream().Write(data, 0, data.Length)
  
  let res = req.GetResponse();
  let resStream = res.GetResponseStream();
  let release = CreateRelease.Load(new System.IO.StreamReader(resStream))

  let uploadURL = sprintf "https://uploads.github.com/repos/BoltEngine/bolt/releases/%A/assets?name=%s_Binary.zip" release.Id buildName
  let uploadData = System.IO.File.ReadAllBytes(sprintf "./upload/%s.zip" buildName);

  postFile uploadURL uploadData
)

"CreatePackage" 
  ==> "UploadPackage"

Run "UploadPackage"