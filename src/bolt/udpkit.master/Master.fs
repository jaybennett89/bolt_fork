namespace UdpKit.Master

open System
open System.Net
open System.Net.Sockets
open System.Threading

open UdpKit

module Master =

  // starts a new peer mailbox
  let private StartPeer (context:MasterContext) (game:Game) (peerId:Guid) =
    PeerMailbox.Start(fun inbox ->
      let rec loop (peer:Peer) =
        async {
          try 
            let! msg = inbox.Receive(if peer.IsHost then context.HostTimeout else context.ClientTimeout)

            match msg with
            | PeerMessage.Shutdown -> 
              context.PeerRemove game peerId
              return ()

            | PeerMessage.MessageReceived(msg, args) ->
              return! loop <| peer.HandleMessage msg args

            | PeerMessage.BeginNatPunch(otherId, otherInbox, otherNat) as nat ->
              return! loop <| peer.BeginNatPunch otherId otherInbox otherNat

            | PeerMessage.Error(text) ->
              UdpLog.Error(text)
              return! loop peer

            | PeerMessage.PerformPunchOnce(remoteId, remoteEndPoint, selfEndPoint) ->
              let msg = context.Protocol.CreateMessage<Protocol.PunchOnce>()
              msg.RemotePeerId <- remoteId
              msg.RemoteEndPoint <- remoteEndPoint
              context.Socket.Send(EndPoint.toDotNet selfEndPoint, msg)
              return! loop peer
              
            | PeerMessage.PerformDirectConnection(remoteId, remoteEndPoint, selfEndPoint) ->
              let msg = context.Protocol.CreateMessage<Protocol.DirectConnection>()
              msg.RemotePeerId <- remoteId
              msg.RemoteEndPoint <- remoteEndPoint
              context.Socket.Send(EndPoint.toDotNet selfEndPoint, msg)
              return! loop peer

          with
            | ex ->  
              context.PeerRemove game peerId
              UdpLog.Error(ex.ToString())
              
          return ()
        }

      UdpLog.Info (sprintf "Peer %A Connected" peerId)
      loop {Context=context; Game=game; PeerId=peerId; NatFeatures=None; IsHost=false; Mailbox=inbox}
    )

  // starts a new master mailbox
  let Start (context:MasterContext) = 

    // start master mailbox
    let mailbox = MasterMailbox.Start(fun inbox ->
      let context = ref Unchecked.defaultof<MasterContext>
      let createPeer (g:Game) (peerId:Guid) = StartPeer !context g peerId

      let rec loop () =
        async {
          // wait for args to receive
          let! args = inbox.Receive()

          try 
            match args with
            | MasterMessage.Shutdown -> 
              return ()

            | MasterMessage.Packet (args) ->
              // parse into a message
              let msg = (!context).Protocol.ParseMessage(args.Buffer)

              // grab peer
              match (!context).PeerFind msg createPeer with
              | None -> ()
              | Some peer -> peer.Post(MessageReceived(msg, args))

            | MasterMessage.Context(c) ->
              context := (c :?> MasterContext)

          with
            | ex -> 
              UdpLog.Error(ex.ToString())

          return! loop () 
        }

      // start loop
      loop ()
    )

    // start master socket
    let socket = new AsyncUdpSocket(fun args -> mailbox.Post(MasterMessage.Packet(args)))
    
    // send this socket to the master mailbox
    mailbox.Post(Context({context with Socket=socket}))

    // start socket
    socket.Bind(context.Master)

    // log this
    UdpLog.Info "MasterServer started"

    // return both
    (socket, mailbox)

type PeerLookup (allowedGameIds:Set<Guid>) =

  let createGame id =
    {GameId=id; Peers=new PeerDictionary(); Hosts=new HostDictionary()}

  let lookup = 
    allowedGameIds
    |> Seq.map (fun id -> id, createGame id)
    |> Map.ofSeq
    |> ref

  member x.Remove (game:Game) (peerId:Guid) =
    match !lookup |> Map.tryFind game.GameId with
    | None -> ()
    | Some game ->
      let success, removed = game.Peers.TryRemove peerId
      
      if success then
        UdpLog.Info (sprintf "Removed Peer %A" peerId)

        let success, removed = game.Hosts.TryRemove peerId

        if success then 
          UdpLog.Info (sprintf "Removed Host %A" peerId)

  member x.Find (msg:Protocol.Message) (new':Game -> Guid -> PeerMailbox) = 
    if allowedGameIds.Count = 0 then
      UdpLog.Warn (sprintf "Created Game %A" msg.GameId)
      lookup := !lookup  |> Map.add msg.GameId (createGame msg.GameId)

    match !lookup |> Map.tryFind msg.GameId with
    | None -> 
      UdpLog.Info (sprintf "Invalid Game Id %A" msg.GameId)
      None

    | Some game ->
      let success, peer = game.Peers.TryGetValue(msg.PeerId)

      if success then 
        Some peer

      else
        game.Peers.GetOrAdd(msg.PeerId, new' game) |> Some