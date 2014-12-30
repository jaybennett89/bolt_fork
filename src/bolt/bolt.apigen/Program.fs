open Mono.Cecil

open System
open System.IO
open System.Xml
open System.Text
open System.Reflection
open System.Text.RegularExpressions
open System.Collections.Generic

// define our xml type provider
type XML = FSharp.Data.XmlProvider<"..\\..\\..\\build\\bolt.XML">

let remove regex name =
  Regex.Replace(name, regex, "")
    
let replace (regex:string) (value:string) (text:string) =
  Regex.Replace(text, regex, value)

let grab regex text =
  Regex.Match(text, regex).Groups.[1].Value
  
let typeLink (url:string) (text:string) =
  sprintf "[%s](%s)" text url

let typeString (s:string) =
  let s = s.Trim()

  if s.StartsWith("UnityEngine.") then
    typeLink (sprintf "http://docs.unity3d.com/ScriptReference/%s.html" (remove "UnityEngine." s)) s

  else
    match s with
    | "System.Single" -> typeLink "http://msdn.microsoft.com/en-us/library/b1e65aza.aspx" "float"
    | "System.Boolean" -> typeLink "http://msdn.microsoft.com/en-us/library/c8f5xwh7.aspx" "bool"
    | "System.Void" -> typeLink "http://msdn.microsoft.com/en-us/library/yah0tteb.aspx" "void"
    | "System.Object" -> typeLink "http://msdn.microsoft.com/en-us/library/9kkx3h3c.aspx" "object"
    | "System.Int32" -> typeLink "http://msdn.microsoft.com/en-us/library/5kzh1b5w.aspx" "int"
    | "System.UInt32" -> typeLink "http://msdn.microsoft.com/en-us/library/x0sksh43.aspx" "uint"
    | "System.String" -> typeLink "http://msdn.microsoft.com/en-us/library/362314fe.aspx" "string"
    | _ -> 
      System.Web.HttpUtility.HtmlEncode(s)

type DocParam = {
  Xml : XML.Param
  TypeName : string
} with
  member x.Name = x.Xml.Name
  member x.Type = x.TypeName
  
(*
type DocTypeParam = {
  Xml : XML.Typeparam
} with
  member x.Name = x.Xml.Name

  static member ParamList (parms:DocTypeParam list option) = 
    match parms with
    | None -> ""
    | Some parms -> "<" + System.String.Join(", ", parms |> Seq.map (fun x -> x.Name) |> Seq.toArray) + ">"
*)

type DocMethod = {
  Xml : XML.Member
  Cecil : Mono.Cecil.MethodDefinition option
  Type : DocType option
  Parameters : DocParam list
  TypeParams : string list 
} with
  member x.Name = x.Cecil.Value.Name
  member x.Signature = 
    x.Parameters 
      |> List.map (fun p -> (typeString p.TypeName) + " " + p.Name) 
      |> (fun s -> String.Join(", ", s))

and DocProperty = {
  Xml : XML.Member
  Cecil : Mono.Cecil.PropertyDefinition option
  Type : DocType option
} with
  member x.Name = x.Cecil.Value.Name

and DocType = {
  Xml : XML.Member
  Cecil : Mono.Cecil.TypeDefinition
  Methods : DocMethod list
  Properties : DocProperty list
  TypeParams : string list
} with
  member x.Name = x.Xml.Name.Substring(2)

[<EntryPoint>]
let main argv = 

  let sb = ref (new System.Text.StringBuilder())
  let h1 h = (!sb).AppendLine("# " + h) |> ignore
  let h2 h = (!sb).AppendLine("## " + h) |> ignore
  let h3 h = (!sb).AppendLine("### " + h) |> ignore
  let h4 h = (!sb).AppendLine("#### " + h) |> ignore
  let h5 h = (!sb).AppendLine("##### " + h) |> ignore


  let typeName (c:Mono.Cecil.TypeReference) =

    if c.Name.StartsWith("IEnumerable`1") then
      let gparams = String.Join(", ", c.GenericParameters |> Seq.map (fun p -> p.FullName))
      sprintf "IEnumerable&lt;%s&gt;" gparams

    else
      typeString c.FullName
  
  let startsWith c (m:XML.Member) =
    m.Name.StartsWith(c)

  let clean (s:string) =
    Regex.Replace(s, @" {12}", "")

  let append s = 
    (!sb).Append(Regex.Replace(s, "[\n\r\s\t]+", " ")) |> ignore

  let p s = 
    (!sb).AppendLine(Regex.Replace(s, "[\n\r\s\t]+", " ")) |> ignore

  let pre s =
    (!sb).AppendLine(clean s) |> ignore
    
  let code s = 
    (!sb).AppendLine("`" + Regex.Replace(s, "[\n\r\s\t]+", " ") + "`") |> ignore

  let nl () =
    (!sb).AppendLine() |> ignore

  // load xml doc comments
  let xml = XML.Parse(System.IO.File.ReadAllText("..\\..\\..\\..\\..\\build\\bolt.XML"))

  // load dll through cecil
  let dll = Mono.Cecil.AssemblyDefinition.ReadAssembly("..\\..\\..\\..\\..\\build\\bolt.dll")

  // list of all types with doc comments
  let types = new List<DocType>()

  let printSummary (t:XML.Member) =
    match t.Summary with
    | Some s -> p s
    | None -> ()

    match t.Example with
    | Some s -> pre s
    | None -> ()

  let methods =
    xml.Members
    |> Seq.filter (startsWith "M:")
    |> Seq.map (fun x -> {DocMethod.Xml=x; Type=None; Cecil=None; Parameters=[]; TypeParams=[]}) 
    |> Seq.map (fun x ->

      let paramTypes =
        Regex.Match(x.Xml.Name, @"\((.*?)\)").Groups.[1].Value.Split(',')
        |> Seq.map (fun s ->  s.Replace("@", ""))

      let parameters = 
        x.Xml.Params 
        |> Seq.zip paramTypes
        |> Seq.map (fun (t, p) -> {DocParam.Xml = p; TypeName=t})
        |> Seq.toList

//      let typeParams = 
//        match x.Xml.Typeparam with
//        | None -> []
//        | Some t -> [{DocTypeParam.Xml = t}]

      {x with Parameters=parameters; TypeParams=[]}
    )
    |> Seq.groupBy (fun x -> Regex.Match(x.Xml.Name, @"(?<=M:)(Bolt\.|BoltInternal\.|)[^.]+").Value)
    |> Seq.map (fun (c, m) -> (c, m |> Seq.toList))
    |> Map.ofSeq

  let properties =
    xml.Members
    |> Seq.filter (startsWith "P:")
    |> Seq.map (fun x -> {DocProperty.Xml=x; Cecil=None; Type=None})
    |> Seq.groupBy (fun x -> Regex.Match(x.Xml.Name, @"(?<=P:)(Bolt\.|BoltInternal\.|)[^.]+").Value)
    |> Seq.map (fun (c, m) -> (c, m |> Seq.toList))
    |> Map.ofSeq

  let types = 
    xml.Members
    |> Seq.filter (startsWith "T:")
    |> Seq.map (fun x -> {Xml=x; Cecil=null; Methods=[]; Properties=[]; TypeParams=[]})

    // find cecil type
    |> Seq.choose (fun x ->
      let cecil = dll.MainModule.Types |> Seq.tryFind (fun t -> t.FullName = x.Name)

      match cecil with
      | None -> None
      | Some c -> Some {x with Cecil=c}
    )

//    // find type params
//    |> Seq.map (fun x ->
//      match x.Xml.Typeparam with
//      | None -> x
//      | Some tp -> {x with TypeParams = [{DocTypeParam.Xml = tp}]}
//    )

    // find methods
    |> Seq.map (fun x ->
      match methods.TryFind x.Name with
      | None -> x
      | Some m -> 
        let m = 
          m |> List.map (fun m ->
            let n = m.Xml.Name.Trim()
            let rmatch = Regex.Match(n, @"([a-zA-Z0-9]+)(\(|$)");
            {m with Cecil = Some (x.Cecil.Methods |> Seq.find (fun m -> m.Name = rmatch.Groups.[1].Value))}
          )

        {x with Methods=m}
    )

    // find properties
    |> Seq.map (fun x ->
      match properties.TryFind x.Name with
      | None -> x
      | Some p -> 
        let p = 
          p |> List.map (fun m ->
            let n = m.Xml.Name.Trim()
            let rmatch = Regex.Match(n, @"([a-zA-Z0-9]+)$");
            {m with Cecil = Some (x.Cecil.Properties |> Seq.find (fun p -> p.Name = rmatch.Groups.[1].Value))}
          )

        {x with Properties=p}
    )

    // filter out types without a cecil type
    |> Seq.toArray

  p "*This document is auto-generated by the bolt.apigen tool, do not edit this directly as the changes will be lost.*"

  h2 "Table of Contents"

  for t in types do
    p (sprintf "* [%s](#%s)" t.Name t.Name)

    // PROPERTIES

    for m in t.Properties |> Seq.distinctBy (fun p -> p.Name) do
      p (sprintf "  * [%s](#%s)" m.Name m.Name)
  
    // METHODS

    for m in t.Methods |> Seq.distinctBy (fun p -> p.Name) do
      p (sprintf "  * [%s()](#%s)" m.Name m.Name)

  nl()
  h2 "Documentation"

  for t in types do
    h3 (sprintf "<a id=\"%s\"></a> %s" t.Name t.Name)
    printSummary t.Xml

    if t.Properties.Length > 0 then 
      h4 "Properties"

    for m in t.Properties do

      let set = 
        if m.Cecil.Value.SetMethod <> null && m.Cecil.Value.SetMethod.IsPublic
          then "set; "
          else ""

      h5 (sprintf "public %s %s { get; %s}" (typeName m.Cecil.Value.PropertyType) m.Name set)
      printSummary m.Xml
      
    if t.Methods.Length > 0 then 
      h4 "Methods"

    for m in t.Methods do
      h5 (sprintf "public %s %s(%s)" (typeName m.Cecil.Value.ReturnType) m.Name m.Signature)
      printSummary m.Xml

  File.WriteAllText("..\\..\\..\\..\\..\\build\\bolt.md", (!sb).ToString());
  
  0