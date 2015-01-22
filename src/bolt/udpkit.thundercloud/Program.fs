﻿open System
open System.IO
open System.Net

open Microsoft.FSharp.Text
open UdpKit.Master

type Config = FSharp.Data.XmlProvider<"ConfigSample.xml">

type Ports = {
  Master : int
  Probe0 : int
  Probe1 : int
  Probe2 : int
} with
  member x.Next =
    {x with 
      Master = x.Master + 1
      Probe0 = x.Probe0 + 1
      Probe1 = x.Probe1 + 1
      Probe2 = x.Probe2 + 1
    }

type MasterObjects = {
  Probe : Probe.T
  Master : AsyncUdpSocket * MasterMailbox
}

let startMaster (context:MasterContext) (m:Config.Master) = 
  let count = if m.Count.IsNone then 1 else m.Count.Value
  let probe0Ip = IPAddress.Parse(m.NatProbe.Ip0)
  let probe1Ip = IPAddress.Parse(m.NatProbe.Ip1)
  let probe2Ip = m.NatProbe.Ip2 |> Option.map (fun ip -> IPAddress.Parse(ip))
  let masterIp = IPAddress.Parse(m.Ip)

  let rec startAll (ports:Ports) c =
    match c with
    | 0 -> []
    | c ->
      let context = 
        {context with
          Master = new IPEndPoint(masterIp, ports.Master)
          Probe0 = new IPEndPoint(probe0Ip, ports.Probe0)
          Probe1 = new IPEndPoint(probe1Ip, ports.Probe1)
          Probe2 = if probe2Ip.IsNone then context.Probe2 else new IPEndPoint(probe2Ip.Value, ports.Probe2)
        }

      let probe = Probe.Start context
      let master = Master.Start context
      
      // start master
      {Master=master; Probe=probe} :: (startAll ports.Next (c-1))

  startAll {Master=24000; Probe0=25000; Probe1=26000; Probe2=27000} count

[<EntryPoint>]
let main argv = 
  UdpKit.UdpLog.SetWriter(fun i m -> Console.WriteLine(m))

  let path = 
    let asm = System.Reflection.Assembly.GetEntryAssembly()
    let location = asm.Location
    let fileInfo = new FileInfo(location)
    Path.Combine(fileInfo.Directory.FullName, "Config.xml")

  let configFile = ref path
  let logLevel = ref 4
  let showUsage = ref false

  let specs = 
    [
      "--config", ArgType.String(fun s -> configFile := s), "specifies the configuration file to use"
      "--log", ArgType.Int(fun i -> logLevel := i), "logging level, 1 = Errors only, 2 = +Warnings, 3 = +Info, 4 = +Debug"
    ] |> List.map (fun (sh, ty, desc) -> ArgInfo(sh, ty, desc))

  let compile text = 
    match specs |> List.tryFind (fun arg -> arg.Name = text) with
    | None -> showUsage := true
    | Some _ -> ()
    
  ArgParser.Parse(specs, compile)

  if !showUsage then
    ArgParser.Usage(specs)

  else
    UdpKit.UdpLog.Info (sprintf "Reading Config File: %s" !configFile)
    let cfg = Config.Parse(System.IO.File.ReadAllText(!configFile))
    let gameIdSet = cfg.GameIds |> Set.ofArray
    let peerLookup = new PeerLookup(gameIdSet)

    let context = 
      {
        PeerFind = peerLookup.Find 
        PeerRemove = peerLookup.Remove

        HostTimeout = cfg.Timeouts.Host
        ClientTimeout = cfg.Timeouts.Client

        Probe0 = new IPEndPoint(IPAddress.Any, 0)
        Probe1 = new IPEndPoint(IPAddress.Any, 0)
        Probe2 = new IPEndPoint(IPAddress.Any, 0)
        Master = new IPEndPoint(IPAddress.Any, 0)

        Socket = Unchecked.defaultof<AsyncUdpSocket>
        Protocol = new UdpKit.Protocol.Context(System.Guid.NewGuid())
        LanNetmask = UdpKit.UdpIPv4Address.Parse(cfg.LanNetmask.Netmask)
      }

    let allMasters = 
      cfg.Masters 
      |> Seq.map (startMaster context) 
      |> Seq.toList

    while true do 
      System.Console.ReadKey(true) |> ignore
  0
