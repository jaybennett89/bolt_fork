namespace UdpKit.Master

open System
open System.Net
open System.Net.Sockets
open System.Threading

open UdpKit


module Punch =
  
  let Start (context:MasterContext) =

    let introductions = 
      ref Map.empty<Guid, PunchIntroduction>

    let mailbox = PunchMailbox.Start (fun inbox ->
      let rec loop () =
        async {
          let! msg = inbox.Receive()

          try 
            
            match msg with
            | Shutdown -> 
              return ()

            | Begin (key, a, b) ->
              introductions :=  !introductions |> Map.add key (PunchIntroduction.New key a b)
              UdpLog.Info (sprintf "Beginning nat-punch attempt between %A and %A" a.PeerId b.PeerId)

            | Packet (packet) ->
              let msg = context.Protocol.ParseMessage(packet.Buffer)

              ()

              //match msg with
              //| :? obj -> ()

          with
            | ex ->
              UdpLog.Error(ex.ToString())

          return! loop ()
        }

      loop ()
    )
    
    // start punch socket
    let socket = new AsyncUdpSocket(fun args -> mailbox.Post(Packet(args)))
    socket.Bind(context)

    // return this
    (socket, mailbox)
