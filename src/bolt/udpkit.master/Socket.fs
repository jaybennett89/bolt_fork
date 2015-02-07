namespace UdpKit.Master

open System
open System.Net
open System.Net.Sockets
open System.Threading

open UdpKit

module EndPoint =
  let toUdpKit (ep:EndPoint) = 
    let ep = ep :?> IPEndPoint
    new UdpEndPoint(new UdpIPv4Address(ep.Address.Address), uint16 ep.Port);

  let toDotNet (ep:UdpEndPoint) = 
    new IPEndPoint(new IPAddress([| ep.Address.Byte3; ep.Address.Byte2; ep.Address.Byte1; ep.Address.Byte0 |]), int32 ep.Port);

type SocketData = {
  Data : byte array
  Size : int
  Remote : EndPoint
  Reply : Protocol.Message -> unit
} with
  static member New () =
    {Data=Array.zeroCreate 1024; Size=0; Remote=null; Reply=Unchecked.defaultof<Protocol.Message -> unit>}

type AsyncUdpSocket (onRecv:SocketData -> unit) =

  let socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
  let sendQueue = new System.Collections.Concurrent.ConcurrentQueue<SocketData>()

  let queueMsg (remote:EndPoint) (msg:Protocol.Message) =
    let data = SocketData.New()
    let size = msg.Context.WriteMessage(msg, data.Data)
    sendQueue.Enqueue({data with Size=size; Remote=remote})
  
  let loop () =

    while true do 
      try 
      
        let rec send () =
          try 
            if sendQueue.Count > 0 then
              let ok, data = sendQueue.TryDequeue() 

              if ok then
                UdpLog.Info (sprintf "%A: Send To:%A" socket.LocalEndPoint data.Remote)
                socket.SendTo(data.Data, data.Size, SocketFlags.None, data.Remote) |> ignore
                send()

              else
                ()

            else
              ()
          with
          | ex -> 
            UdpLog.Error(ex.Message);
            UdpLog.Error(ex.StackTrace);
            send()

        let rec recv wait =
          try 
            if socket.Poll(wait, SelectMode.SelectRead) then
               let data = SocketData.New()
               let mutable remote = new IPEndPoint(IPAddress.Any, 0) :> EndPoint
               let bytes = socket.ReceiveFrom(data.Data, &remote)

               UdpLog.Info (sprintf "%A: Recv From:%A" socket.LocalEndPoint remote)

               onRecv {data with Size = bytes; Remote = remote; Reply = queueMsg remote}

               // once more
               recv 0

            else

              ()

          with
          | ex -> 
            UdpLog.Error(ex.Message);
            UdpLog.Error(ex.StackTrace);
            recv 0

        // send data
        send ()
      
        // recv data
        recv 1

      with
      | ex ->
        UdpLog.Error(ex.Message)
        UdpLog.Error(ex.StackTrace)
    
      // yield thread
      System.Threading.Thread.Yield() |> ignore

  let thread = new Thread(loop)
      
  let bind (endpoint:IPEndPoint) =
    // bind socket
    socket.Bind(endpoint)
    socket.Blocking <- false
    
    thread.IsBackground <- true
    thread.Start()

    // log that this happened
    UdpLog.Info (sprintf "UdpAsyncSocket bound to %A" socket.LocalEndPoint)

  member x.EndPoint = socket.LocalEndPoint :?> IPEndPoint
  member x.Bind (endpoint:IPEndPoint) = bind endpoint
  member x.Send (data:SocketData) = sendQueue.Enqueue(data)
  member x.Send (endpoint:EndPoint, msg:Protocol.Message) = queueMsg endpoint msg
