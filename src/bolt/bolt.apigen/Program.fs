open Mono.Cecil

open System
open System.IO
open System.Xml
open System.Text
open System.Reflection
open System.Text.RegularExpressions
open System.Collections.Generic

let remove regex name =
  Regex.Replace(name, regex, "")
    
let replace (regex:string) (value:string) (text:string) =
  Regex.Replace(text, regex, value)

let grab regex text =
  Regex.Match(text, regex).Groups.[1].Value

[<EntryPoint>]
let main argv = 

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

  let sb = ref (new System.Text.StringBuilder())
  let h1 h = (!sb).AppendLine("# " + h) |> ignore
  let h2 h = (!sb).AppendLine("## " + h) |> ignore
  let h3 h = (!sb).AppendLine("### " + h) |> ignore
  let h4 h = (!sb).AppendLine("#### " + h) |> ignore
  
  let a s = 
    (!sb).Append(Regex.Replace(s, "[\n\r\s\t]+", " ")) |> ignore

  let p s = 
    (!sb).AppendLine(Regex.Replace(s, "[\n\r\s\t]+", " ")) |> ignore
    
  let code s = 
    (!sb).AppendLine("`" + Regex.Replace(s, "[\n\r\s\t]+", " ") + "`") |> ignore

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

  0