namespace UdpKit.Master

open System
open System.Net
open System.Net.Sockets
open System.Threading

open UdpKit

module Probe = 

  type T (context:MasterContext) =

    let onRecv (probe:int) (fwd:AsyncUdpSocket option ref) (args:SocketAsyncEventArgs) = 
      try 
        match context.Protocol.ParseMessage(args.Buffer) with
        | :? Protocol.ProbeEndPoint as query -> 

          match !fwd with
          | None -> ()
          | Some fwd ->
            let msg = context.Protocol.CreateMessage<Protocol.ProbeUnsolicited>()
            msg.WanEndPoint <- args.RemoteEndPoint |> EndPoint.toUdpKit
            fwd.Send(args.RemoteEndPoint, msg)
          
          // setup message
          let msg = context.Protocol.CreateMessage<Protocol.ProbeEndPointResult>(query)
          msg.WanEndPoint <- args.RemoteEndPoint |> EndPoint.toUdpKit
          Args.Reply args msg

        | _ ->
          Args.Push args

      with
        | ex -> 
          UdpLog.Error(ex.ToString())
    
    let socket2 = ref (Some(new AsyncUdpSocket(onRecv 2 (ref None))))
    let socket1 = new AsyncUdpSocket(onRecv 1 (ref None))
    let socket0 = new AsyncUdpSocket(onRecv 0 socket2)

    member x.Probe0 = socket0.EndPoint
    member x.Probe1 = socket1.EndPoint
    member x.Probe2 = 
      match !socket2 with
      | None -> new IPEndPoint(IPAddress.Any, 0)
      | Some s -> s.EndPoint
  
    member x.Start () =
      if context.Probe2.Address = IPAddress.Any then
        socket2 := None

      else
        socket2.Value.Value.Bind context.Probe2
      
      socket1.Bind context.Probe1
      socket0.Bind context.Probe0

      UdpLog.Info ("NatProbe started")

  let Start (context:MasterContext) = 
    let natprobe = new T(context)
    natprobe.Start()
    natprobe

  
