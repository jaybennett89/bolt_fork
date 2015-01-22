namespace UdpKit.Master

open System
open System.Net
open System.Net.Sockets
open System.Threading

open UdpKit

type PeerMessage
  = Shutdown
  | MessageReceived of Protocol.Message * SocketAsyncEventArgs
  | BeginNatPunch of Guid * MailboxProcessor<PeerMessage> * NatFeatures
  | PerformDirectConnectionWan of Guid * UdpEndPoint * UdpEndPoint
  | PerformDirectConnectionLan of Guid * UdpEndPoint * UdpEndPoint
  | PerformPunchOnce of Guid * UdpEndPoint * UdpEndPoint
  | Error of string

type MasterMessage 
  = Shutdown
  | Packet of SocketAsyncEventArgs
  | Context of obj
  
type PeerMailbox = MailboxProcessor<PeerMessage>
type MasterMailbox = MailboxProcessor<MasterMessage>

type Host = {
  PeerId : Guid
  Session : UdpSession
}

type HostDictionary = System.Collections.Concurrent.ConcurrentDictionary<Guid, Host>
type PeerDictionary = System.Collections.Concurrent.ConcurrentDictionary<Guid, PeerMailbox>

type Game = {
  GameId : Guid
  Peers : PeerDictionary
  Hosts : HostDictionary
}
  
type MasterContext = {
  PeerFind : Protocol.Message -> (Game -> Guid -> PeerMailbox) -> PeerMailbox option
  PeerRemove : Game -> Guid -> unit

  ClientTimeout : int
  HostTimeout : int

  Probe0 : IPEndPoint
  Probe1 : IPEndPoint
  Probe2 : IPEndPoint
  Master : IPEndPoint

  Socket : AsyncUdpSocket
  Protocol : Protocol.Context
  LanNetmask : UdpIPv4Address
}

type Peer = {
  Game : Game
  IsHost : bool
  PeerId : Guid
  Mailbox : PeerMailbox
  Context : MasterContext
  NatFeatures : UdpKit.NatFeatures option
} with
  member x.HandleMessage (msg:Protocol.Message) (args:SocketAsyncEventArgs) =
    match msg with
    | :? Protocol.PeerConnect as connect -> x.OnPeerConnect connect args
    | :? Protocol.PeerDisconnect as disconnect -> x.OnPeerDisconnect disconnect args
    | :? Protocol.ProbeFeatures as features -> x.OnProbeFeatures features args
    | :? Protocol.HostRegister as register -> x.OnHostRegister register args
    | :? Protocol.HostKeepAlive as keepalive -> x
    | :? Protocol.GetHostList as getlist -> x.OnGetHostList getlist args
    | :? Protocol.PunchRequest as request -> x.OnPunchRequest request args
    | _ ->
      failwithf "Unknown message type %s" (msg.GetType().Name)

  member x.BeginNatPunch (otherId:Guid) (otherInbox:MailboxProcessor<PeerMessage>) (otherNat:NatFeatures) =
    match x.NatFeatures with
    | None ->
      otherInbox.Post(Error(sprintf "Can't connect to host %A, it does not have a valid NAT state" x.PeerId))

    | Some nat -> 
      let bothHaveWan = nat.WanEndPoint.IsWan && otherNat.WanEndPoint.IsWan
      let bothHaveLan = nat.LanEndPoint.IsLan && otherNat.LanEndPoint.IsLan
      let bothHaveSameWan = UdpIPv4Address.op_Equality(nat.WanEndPoint.Address, otherNat.WanEndPoint.Address)
      let bothHaveSameLanSubnet = 
        UdpIPv4Address.op_Equality(
          UdpIPv4Address.op_BitwiseAnd(nat.LanEndPoint.Address, x.Context.LanNetmask),
          UdpIPv4Address.op_BitwiseAnd(otherNat.LanEndPoint.Address, x.Context.LanNetmask)
        )

      if bothHaveWan && bothHaveLan && bothHaveSameWan && bothHaveSameLanSubnet then
        let bothHaveLanSameAddress = UdpIPv4Address.op_Equality(nat.LanEndPoint.Address, otherNat.LanEndPoint.Address)
        
        // connecting to your own computer
        if bothHaveLanSameAddress then
          otherInbox.Post(PerformDirectConnectionLan(x.PeerId, new UdpEndPoint(UdpIPv4Address.Localhost, nat.LanEndPoint.Port), otherNat.WanEndPoint))

        // connecting to another computer on your lan
        else
          otherInbox.Post(PerformDirectConnectionLan(x.PeerId, nat.LanEndPoint, otherNat.WanEndPoint))

      else
        match nat.WanEndPoint.IsWan, nat.AllowsUnsolicitedTraffic, nat.SupportsEndPointPreservation, otherNat.SupportsEndPointPreservation with
        | false, _, _, _ ->
          otherInbox.Post(Error(sprintf "Can't connect to host %A, it does not have a valid WAN end-point" x.PeerId))

        | true, NatFeatureStates.Yes, _, _ ->
          otherInbox.Post(PerformDirectConnectionWan(x.PeerId, nat.WanEndPoint, otherNat.WanEndPoint))

        | true, _, NatFeatureStates.Yes, NatFeatureStates.Yes ->
          // tell ourselves to do this
          x.Mailbox.Post(PerformPunchOnce(otherId, otherNat.WanEndPoint, nat.WanEndPoint))

          // tell other end to do this
          otherInbox.Post(PerformPunchOnce(x.PeerId, nat.WanEndPoint, otherNat.WanEndPoint))

        | _, _, NatFeatureStates.No, NatFeatureStates.Yes ->
          otherInbox.Post(Error(sprintf "Can't connect to host %A, it does not support NAT-punchthrough" x.PeerId))

        | _, _, NatFeatureStates.Yes, NatFeatureStates.No ->
          otherInbox.Post(Error(sprintf "Can't connect to host %A, your connection does not support NAT-punchthrough and the host does not allow direct connections" x.PeerId))

        | _ ->
          otherInbox.Post(Error(sprintf "Can't connect to host %A," x.PeerId))
    x
        

  member private x.AckMessage (msg:Protocol.Query) (args:SocketAsyncEventArgs) =
    Args.Reply args (x.Context.Protocol.CreateMessage<Protocol.Ack>(msg))

  member private x.OnPeerConnect (connect:Protocol.PeerConnect) (args:SocketAsyncEventArgs) =
    let msg = x.Context.Protocol.CreateMessage<Protocol.PeerConnectResult>(connect)
    msg.Probe0 <- x.Context.Probe0 |> EndPoint.toUdpKit
    msg.Probe1 <- x.Context.Probe1 |> EndPoint.toUdpKit
    msg.Probe2 <- x.Context.Probe2 |> EndPoint.toUdpKit

    Args.Reply args msg

    x
    
  member private x.OnPeerDisconnect (connect:Protocol.PeerDisconnect) (args:SocketAsyncEventArgs) =
    failwith "Peer Disconnected"

  member private x.OnProbeFeatures (features:Protocol.ProbeFeatures) (args:SocketAsyncEventArgs) =
    // ack this message
    x.AckMessage features args

    // update this peer
    {x with NatFeatures = Some(features.NatFeatures)}

  member private x.OnHostRegister (register:Protocol.HostRegister) (args:SocketAsyncEventArgs) =
    // create host object
    let host = {PeerId=x.PeerId; Session=register.Host}

    // add to this games hosts list
    x.Game.Hosts.AddOrUpdate(x.PeerId, host, fun _ _ -> host) |> ignore
    
    // ack message
    Args.Reply args (x.Context.Protocol.CreateMessage<Protocol.Ack>(register))
    
    // mark this peer as being a host
    {x with IsHost=true}
    
  member private x.OnGetHostList (getlist:Protocol.GetHostList) (recvArgs:SocketAsyncEventArgs) =
    let send = recvArgs.UserToken :?> (SocketAsyncEventArgs -> unit)
    let remoteEndpoint = recvArgs.RemoteEndPoint

    x.AckMessage getlist recvArgs

    UdpLog.Info (sprintf "Found %i Hosts" x.Game.Hosts.Count)

    for host in x.Game.Hosts.Values do
      let msg = x.Context.Protocol.CreateMessage<Protocol.HostInfo>()
      msg.Host <- host.Session

      let args = Args.Pop()
      let size = x.Context.Protocol.WriteMessage(msg, args.Buffer)

      args.SetBuffer(0, size)
      args.RemoteEndPoint <- remoteEndpoint

      send args

    x
    
  member private x.OnPunchRequest (request:Protocol.PunchRequest) (args:SocketAsyncEventArgs) =
    match x.NatFeatures with
    | None ->
      x.Mailbox.Post(Error(sprintf "Can't connect to %A. Your connection does not have a valid NAT state" request.Host))

    | Some nat ->
      let ok, other = x.Game.Peers.TryGetValue(request.Host)

      if ok then
        other.Post(PeerMessage.BeginNatPunch(x.PeerId, x.Mailbox, nat))

      else
        x.Mailbox.Post(Error(sprintf "Can't connect to %A. Host not found" request.Host))

    x
    
and PunchMessage 
  = Shutdown
  | Begin of Guid * Peer * Peer
  | Packet of SocketAsyncEventArgs

and PunchMailbox = 
  MailboxProcessor<PunchMessage>

type PunchState 
  = Waiting
  | Ping
  | Ready

type PunchInfo = {
  Peer : Peer
  State : PunchState
  Ping : uint32
  Lan : UdpEndPoint
  Wan : UdpEndPoint
} with
  static member New (p:Peer) =
    {State=Waiting; Ping=0u; Lan=UdpEndPoint.Any; Wan=UdpEndPoint.Any; Peer=p}

type PunchIntroduction = {
  Key : Guid
  Info : PunchInfo list
  LastSeen : DateTime
} with
  static member New key a b =
    {Key=key; Info=[PunchInfo.New(a); PunchInfo.New(b)]; LastSeen=DateTime.Now}
