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

module Args = 
  
  let private Pool = 
    System.Collections.Concurrent.ConcurrentQueue<SocketAsyncEventArgs>()

  let private Completed (sender:obj) (args:SocketAsyncEventArgs) =
    if args.BytesTransferred > 0 && args.SocketError = SocketError.Success then
      (args.UserToken :?> (SocketAsyncEventArgs -> unit)) args

  let Pop () = 
    let success, args = Pool.TryDequeue()

    if success then 
      args.SetBuffer(args.Buffer, 0, args.Buffer.Length)
      args

    else 
      let args = new SocketAsyncEventArgs()
      args.Completed.AddHandler(new EventHandler<SocketAsyncEventArgs>(Completed))
      args.SetBuffer(Array.zeroCreate 1024, 0, 1024)
      args

  let Push (args:SocketAsyncEventArgs) =
    args.UserToken <- null
    args.RemoteEndPoint <- null
    Pool.Enqueue(args)

  let Reply (recvArgs:SocketAsyncEventArgs) (msg:Protocol.Message) =
    // setup send args
    let sendArgs = Pop()
    sendArgs.RemoteEndPoint <- recvArgs.RemoteEndPoint
    sendArgs.SetBuffer(0, msg.Context.WriteMessage(msg, sendArgs.Buffer))

    // reply to this message
    (recvArgs.UserToken :?> (SocketAsyncEventArgs -> unit)) sendArgs

    // return recv args to pool
    Push recvArgs

type AsyncUdpSocket (onRecv:SocketAsyncEventArgs -> unit) =

  // this is the socket we will be using
  let socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)

  // send queue
  let sendQueue = new System.Collections.Concurrent.ConcurrentQueue<SocketAsyncEventArgs>()

  // send counter
  let mutable sendCount = 0;

  // send completed callback
  let rec sendAsyncComplete (args:SocketAsyncEventArgs) =
    Args.Push(args)

    if Interlocked.Decrement(&sendCount) > 0 then
      sendAsync()

  // main send function
  and sendAsync () =
    let success, args = sendQueue.TryDequeue()

    if success then
      args.UserToken <- sendAsyncComplete

      if not <| socket.SendToAsync(args) then
        sendAsyncComplete args
        
  // send queue function
  let send (args:SocketAsyncEventArgs) =
    sendQueue.Enqueue(args)

    if Interlocked.Increment(&sendCount) = 1 then
      sendAsync()

  // recv completed callback
  let rec recvAsyncComplete (args:SocketAsyncEventArgs) = 
    // set reply function
    args.UserToken <- send

    // callback
    onRecv args

    // start next receive op
    recvAsync()

  // main recv function
  and recvAsync () = 
    // pop args object
    let args = Args.Pop()
    args.RemoteEndPoint <- new IPEndPoint(IPAddress.Any, 0)
    args.UserToken <- recvAsyncComplete

    if not <| socket.ReceiveFromAsync(args) then
      recvAsyncComplete args
      
  let bind (endpoint:IPEndPoint) =
    // bind socket
    socket.Bind(endpoint)
    socket.Blocking <- false

    // log that this happened
    UdpLog.Info (sprintf "UdpAsyncSocket bound to %A" socket.LocalEndPoint)

    // start recv loop
    recvAsync()

  member x.EndPoint = socket.LocalEndPoint :?> IPEndPoint
  member x.Bind (endpoint:IPEndPoint) = bind endpoint
  member x.Send (args:SocketAsyncEventArgs) = send args
  member x.Send (endpoint:EndPoint, msg:Protocol.Message) =
    let args = Args.Pop()
    args.RemoteEndPoint <- endpoint
    args.SetBuffer(0, msg.Context.WriteMessage(msg, args.Buffer))
    
    x.Send(args)