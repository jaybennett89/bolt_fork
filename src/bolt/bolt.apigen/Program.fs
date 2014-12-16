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
  
type DocParam = {
  Xml : XML.Param
  TypeName : string
} with
  member x.Name = x.Xml.Name
  member x.Type = x.TypeName
  
type DocTypeParam = {
  Xml : XML.Typeparam
} with
  member x.Name = x.Xml.Name

  static member ParamList (parms:DocTypeParam list option) = 
    match parms with
    | None -> ""
    | Some parms -> "<" + System.String.Join(", ", parms |> Seq.map (fun x -> x.Name) |> Seq.toArray) + ">"

type DocMethod = {
  Xml : XML.Member
  Cecil : Mono.Cecil.MethodDefinition option
  Type : DocType option
  Parameters : DocParam list
  TypeParams : DocTypeParam list
} with
  member x.Name = x.Cecil.Value.Name

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
  TypeParams : DocTypeParam list
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

      let typeParams = 
        match x.Xml.Typeparam with
        | None -> []
        | Some t -> [{DocTypeParam.Xml = t}]

      {x with Parameters=parameters; TypeParams=typeParams}
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

    // find type params
    |> Seq.map (fun x ->
      match x.Xml.Typeparam with
      | None -> x
      | Some tp -> {x with TypeParams = [{DocTypeParam.Xml = tp}]}
    )

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


  h1 "Bolt API Docs"

  p "*This document is auto-generated by the bolt.apigen tool, do not edit this directly as the changes will be lost.*"

  h2 "Table of Contents"

  for t in types do
    p (sprintf "* *Class* [%s](#%s)" t.Name t.Name)

    if t.Properties.Length > 0 then 
      p "  * *Properties*"

    for m in t.Properties do
      p (sprintf "  * [%s](#%s)" m.Name m.Name)
  
    if t.Methods.Length > 0 then 
      p "  * *Methods*"

    for m in t.Methods do
      p (sprintf "  * [%s](#%s)" m.Name m.Name)

  nl()
  h2 "Documentation"

  for t in types do
    h3 (sprintf "<a name=\"%s\"></a> %s" t.Name t.Name)
    
    if t.Properties.Length > 0 then 
      h4 "Properties"

    for m in t.Properties do

      let set = 
        if m.Cecil.Value.SetMethod <> null && m.Cecil.Value.SetMethod.IsPublic
          then "set; "
          else ""

      h5 (sprintf "public %s %s { get; %s}" m.Cecil.Value.PropertyType.Name m.Name set)
      
      match m.Xml.Summary with
      | Some s -> p s
      | None -> ()

      match m.Xml.Example with
      | Some s -> pre s
      | None -> ()

  File.WriteAllText("..\\..\\..\\..\\..\\build\\bolt.md", (!sb).ToString());
    
  0

  (*
  let rec typeNamePretty (t:Mono.Cecil.TypeReference) =
    match t.FullName with
    | "System.Void" -> "void"
    | "System.Single" -> "float"
    | "System.Int32" -> "int"
    | "System.UInt32" -> "uint"
    | "System.String" -> "string"
    | "System.Boolean" -> "bool"
    | "System.Object" -> "object"
    | _ -> 
      if t.IsGenericInstance then
        t.FullName |> remove "`\d+" |> replace "<" "&lt;" |> replace ">" "&gt;"

      else
        if t.Namespace = "UnityEngine" 
          then t.Name
          else t.FullName
  
  let makePath (file:string) =
    let file = file.Replace("&lt;", "[").Replace("&gt;", "]")
    let path = System.IO.Path.GetFullPath("../../../../api/" + file)
    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)) |> ignore
    path

  let save file =
    let text = "*This file is auto-generated, do not edit.*\n\n" + (!sb).ToString()
    File.WriteAllText(makePath file, text);

  let saveIfNotExists file =
    if not (File.Exists(makePath file)) then
      File.WriteAllText(makePath file, (!sb).ToString());

  let includeFile header file =
    if header <> "Summary" then
      h2 header
      
    if File.Exists(makePath file) |> not then
      File.CreateText(makePath file).Close();

    let text = File.ReadAllText(makePath file).Trim()

    if text <> "" then
      (!sb).Append(text) |> ignore

    else
      if header <> "Summary" 
        then p (sprintf "Contents of '%s' is empty" file)
        else a (sprintf "Contents of '%s' is empty" file)

  let dll = 
    Mono.Cecil.AssemblyDefinition.ReadAssembly("C:\\Users\\Fredrik\\Documents\\GitHub\\bolt\\build\\bolt.dll")

  let types = 
    dll.MainModule.Types 
    |> Seq.filter (fun t -> t.IsPublic)
    |> Seq.filter (fun t -> t.CustomAttributes |> Seq.exists (fun a -> a.AttributeType.FullName = "Bolt.DocumentationAttribute"))

  let typeName (t:Mono.Cecil.TypeDefinition) =
    let attr = t.CustomAttributes |> Seq.find (fun a -> a.AttributeType.FullName = "Bolt.DocumentationAttribute")
    let alias = attr.Properties |> Seq.tryFind (fun p -> p.Name = "Alias")
    let name = 
      match alias with
      | None -> t.FullName
      | Some(v) -> v.Argument.Value.ToString()

    name
      .Replace("`1", "<T>")
      .Replace("<", "&lt;")
      .Replace(">", "&gt;")

  let fields (t:Mono.Cecil.TypeDefinition) = t.Fields |> Seq.filter (fun f -> f.IsPublic)
  let properties (t:Mono.Cecil.TypeDefinition) = t.Properties |> Seq.filter (fun p -> p.GetMethod.IsPublic)
  let methods (t:Mono.Cecil.TypeDefinition) = 
    t.Methods 
    |> Seq.filter (fun m -> m.IsPublic && m.Name <> ".ctor")
    |> Seq.filter (fun m -> m.CustomAttributes |> Seq.exists (fun a -> a.AttributeType.FullName = "System.ObsoleteAttribute") |> not)
    |> Seq.filter (fun m -> (not <| m.Name.StartsWith("get_")) && (not <| m.Name.StartsWith("set_")))
  
  let typePath (t:Mono.Cecil.TypeDefinition) = sprintf "Types/%s.md" (typeName t)
  let typeLink (t:Mono.Cecil.TypeDefinition) = sprintf "[%s](%s)" (typeName t) (typePath t)
  let typeInclude header (t:Mono.Cecil.TypeDefinition) = 
    includeFile header (t |> typePath |> replace ".md" ("_" + header + ".md"))

  let memberPath (m:Mono.Cecil.IMemberDefinition) = 
    let memberTypeName = m.GetType().Name.[0].ToString()
    sprintf "%s/%s/%s.md" (typeName m.DeclaringType) memberTypeName m.Name

  let memberLink (m:Mono.Cecil.IMemberDefinition) = 
    sprintf "[%s](%s)" m.Name (memberPath m)
    
  let memberInclude header (m:Mono.Cecil.IMemberDefinition)  = 
    includeFile header ("Types/" + (m |> memberPath |> replace ".md" ("_" + header + ".md")))

  let memberGroup plural singular (members:Mono.Cecil.IMemberDefinition seq) =
      if (members |> Seq.length) > 0 then
        h2 plural
        p (sprintf "| %s | Summary |" singular)
        p "|:-----|:--------|"
        for m in members do 
          m |> memberLink |> sprintf "|%s|" |> a
          m |> memberInclude "Summary"
          p "|"
          
  let isStatic yes =
    if yes then "static " else ""

  let writeReadme () =
    sb := new System.Text.StringBuilder()

    h1 "Bolt API Documentation"

    p "| Type | Summary |"
    p "|:-----|:--------|"

    for t in types do
      t |> typeLink |> sprintf "|%s|" |> a
      t |> typeInclude "Summary"
      p "|"

    save "README.md"

  let writeTypes () =
  
    for t in types do
      sb := new System.Text.StringBuilder()

      h1 (typeName t)
      
      let abs = 
        if t.IsAbstract then "abstract " else ""

      let typ = 
        if t.IsValueType then "struct " 
        elif t.IsClass then "class "
        elif t.IsInterface then "interface "
        elif t.IsEnum then "enum "
        else failwith "unknown type"

      code ("public " + abs + typ + (typeName t))

      t |> typeInclude "Description"
      t |> typeInclude "Example"

      memberGroup "Fields" "Field" (t |> fields |> Seq.cast<Mono.Cecil.IMemberDefinition>)
      memberGroup "Properties" "Property" (t |> properties |> Seq.cast<Mono.Cecil.IMemberDefinition>)
      memberGroup "Methods" "Method" (t |> methods |> Seq.cast<Mono.Cecil.IMemberDefinition>)

      save (typePath t)

  let writeMembers () =
    for t in types do

      for m in fields t do
        sb := new System.Text.StringBuilder()

        h1 (sprintf "%s.%s" (typeLink t) (m.Name))
        
        let typ = typeNamePretty m.FieldType
        let statc = isStatic m.IsStatic
        let readonly = if m.IsInitOnly then "readonly " else ""
        code (sprintf "public %s%s%s %s" readonly statc typ m.Name)
        
        m |> memberInclude "Description"
        m |> memberInclude "Example"

        save ("Types/" + (memberPath m))

      for m in properties t do
        sb := new System.Text.StringBuilder()

        h1 (sprintf "%s.%s" (typeLink t) (m.Name))

        let set = if m.SetMethod <> null then "set; " else ""
        let typ = typeNamePretty m.PropertyType
        let statc = isStatic m.GetMethod.IsStatic
        code (sprintf "public %s%s %s { get; %s}" statc typ m.Name set)
        
        m |> memberInclude "Description"
        m |> memberInclude "Example"

        save ("Types/" + (memberPath m))

      for m in methods t do
        sb := new System.Text.StringBuilder()

        h1 (sprintf "%s.%s" (typeLink t) (m.Name))

        let ret = typeNamePretty m.ReturnType

        let modif = 
          if m.IsStatic then "static "
          elif m.IsVirtual then "virtual "
          else ""

        let parameters = 
          String.Join(", ", m.Parameters |> Seq.map (fun p -> (typeNamePretty p.ParameterType) + " " + p.Name))

        code (sprintf "public %s%s %s(%s)" modif ret m.Name parameters)
        
        if m.Parameters.Count > 0 then
          m |> memberInclude "Parameters"

        m |> memberInclude "Description"
        m |> memberInclude "Example"

        save ("Types/" + (memberPath m))


  writeReadme() 
  writeTypes()
  writeMembers()
  *)