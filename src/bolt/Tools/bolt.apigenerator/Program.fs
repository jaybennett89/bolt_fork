// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open Mono.Cecil
open FSharp.Data

open System
open System.IO
open System.Xml
open System.Text
open System.Reflection
open System.Text.RegularExpressions
open System.Collections.Generic

///<summary>
///lol
///</summary>
module TypeUtils =  

  let rec classStringName (t:Type) =
    let gt = t.GetGenericArguments()

    if gt.Length = 0 then 
      t.Name

    else
      Regex.Replace(t.Name, "`\d+", "") + "<" + String.Join(", ", gt |> Array.map classStringName) + ">"

open TypeUtils

type Doc = XmlProvider<"..\\..\\..\\..\\build\\bolt.XML">

type CParam = {
  xml : Doc.Param
} with
  member x.Name = x.xml.Name
  member x.Docs = 
    match x.xml.Value with
    | None -> None
    | Some s -> Some(s :> obj)

type CMethod = {
  xml : Doc.Member
  mtd : Mono.Cecil.MethodDefinition
} with

  member x.Parameters =
    seq { for p in x.xml.Params do yield {CParam.xml = p} }

type CProperty = {
  xml : Doc.Member
  prop : Mono.Cecil.PropertyDefinition
}

type CType = {
  xml : Doc.Member
  typ : Mono.Cecil.TypeDefinition
  mtds : CMethod array
  props : CProperty array
}

module Utils =
  let assemblies =
    [
      Mono.Cecil.AssemblyDefinition.ReadAssembly(@"C:\Program Files (x86)\Unity\Editor\Data\Mono\lib\mono\2.0\mscorlib.dll")
      Mono.Cecil.AssemblyDefinition.ReadAssembly(@"..\..\..\..\..\..\build\UnityEngine.dll")
      Mono.Cecil.AssemblyDefinition.ReadAssembly(@"..\..\..\..\..\..\build\udpkit.dll")
      Mono.Cecil.AssemblyDefinition.ReadAssembly(@"..\..\..\..\..\..\build\bolt.dll")
    ]

  let remove regex name =
    Regex.Replace(name, regex, "")

  let grab regex text =
    Regex.Match(text, regex).Groups.[1].Value

  let memberName (m:Doc.Member) = 
    m.Name |> grab @"\.([a-zA-Z]+|#ctor)(\(|$|``\d)"

  let findType (name:string) =   
    let mtc = Regex.Match(name, @"(.*?)\{``(\d)\}")

    let name =
      if not mtc.Success then 
        name 
      else
        let n = (int mtc.Groups.[2].Value) + 1
        mtc.Groups.[1].Value + "`" + n.ToString()

    assemblies |> Seq.pick (fun asm ->
      asm.MainModule.Types |> Seq.tryFind (fun t -> t.FullName = name)
    )

  let findClass (m:Doc.Member) =
    let n = 
        let index = m.Name.IndexOf('(')
        if index <> -1 
            then m.Name.Substring(0, index) 
            else m.Name

    let _, count = n |> Seq.countBy (fun c -> c) |> Seq.find (fun (k, c) -> k = '.')
    let regex = sprintf "[TPMEF]:(([a-zA-Z]+(`\d)?\.){%i})" count
    (m.Name |> grab regex).Trim('.') |> findType

  let findProperty (m:Doc.Member) =
    let clss = findClass m
    let name = memberName m
    {
      CProperty.xml = m;
      CProperty.prop = clss.Properties |> Seq.find (fun x -> x.Name = name)
    }

  let findMethod (m:Doc.Member) =
    let clss = findClass m
    let name = memberName m

    let ptypes =
      Regex.Match(m.Name, @"\((.*?)\)").Groups.[1].Value.Split(',')
      |> Seq.filter (fun s -> s <> "")
      |> Seq.map (remove "@")
      |> Seq.map findType
      |> Seq.toArray

    let tpcount =
      let mtc = Regex.Match(m.Name, "`(\d+)")
      if mtc.Success then int mtc.Groups.[1].Value else 0

    let isctor =  name = "#ctor" 
    let bind = BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static
    let methods = clss.Methods |> Seq.filter (fun m -> m.IsConstructor = isctor) |> Seq.toArray

    methods |> Seq.pick (fun mtd ->

      let gm = isctor || tpcount = mtd.GenericParameters.Count
      let nm = isctor || (name = mtd.Name)
      let pm =
        let pms = 
          mtd.Parameters
          |> Seq.map (fun p ->
            if p.ParameterType .IsByReference
              then p.ParameterType.GetElementType()
              else p.ParameterType)
          |> Seq.toArray

        if pms.Length = ptypes.Length 
          then pms |> Seq.zip ptypes |> Seq.forall (fun (a, b) -> a.FullName = b.FullName)
          else false

      if gm && nm && pm 
        then Some {CMethod.xml = m; mtd=mtd}
        else None
    )

open Utils

[<EntryPoint>]
let main argv = 

  let doc = 
    Doc.Parse(System.IO.File.ReadAllText("..\\..\\..\\..\\..\\..\\build\\bolt.XML"))
  
  let methods =
    doc.Members
    |> Seq.filter (fun x -> x.Name.StartsWith("M:"))
    |> Seq.map Utils.findMethod
    |> Seq.toArray
  
  let properties =
    doc.Members
    |> Seq.filter (fun x -> x.Name.StartsWith("P:"))
    |> Seq.map Utils.findProperty
    |> Seq.toArray

  let types =
    doc.Members
    |> Seq.filter (fun x -> x.Name.StartsWith("T:"))
    |> Seq.map (fun m ->
      let type' = Utils.findType (m.Name |> remove "^T:")

      let mtds =
        methods 
          |> Seq.filter (fun m -> m.mtd.DeclaringType.FullName = type'.FullName) 
          |> Seq.sortBy (fun m -> if m.mtd.IsStatic then 0 else 1)
          |> Seq.toArray

      let props =
        properties 
          |> Seq.filter (fun p -> p.prop.DeclaringType.FullName = type'.FullName) 
          |> Seq.sortBy (fun p -> if p.prop.GetMethod.IsStatic then 0 else 1)
          |> Seq.toArray

      {
        CType.xml = m
        CType.typ = type'
        CType.mtds = mtds
        CType.props = props
      }
    )
    |> Seq.sortBy (fun t -> t.typ.Name)
    |> Seq.mapi (fun i t -> (i, t))
    |> Seq.toArray

  0