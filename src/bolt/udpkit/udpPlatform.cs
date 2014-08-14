namespace UdpKit {
  public enum UdpSocketPlatformError {
    None = 0,
    Unknown = 1,
    WouldBlock = 2
  }

  public abstract class UdpPlatform {
    public abstract UdpEndPoint EndPoint { get; }
    public abstract string PlatformErrorString { get; }
    public abstract uint PlatformPrecisionTime { get; }
    public abstract bool IsBroadcasting { get; }
    public abstract bool SupportsBroadcast { get; }

    public abstract bool Close ();
    public abstract bool Bind (UdpEndPoint endpoing);
    public abstract bool RecvPoll (int timeoutInMs);
    public abstract bool RecvFrom (byte[] buffer, int bufferSize, ref int bytesReceived, ref UdpEndPoint remoteEndpoint);
    public abstract bool SendTo (byte[] buffer, int bytesToSend, UdpEndPoint endpoint, ref int bytesSent);

    public abstract void EnableBroadcast (UdpEndPoint address);
    public abstract void DisableBroadcast ();

    public abstract bool RecvBroadcastData (byte[] buffer, out UdpEndPoint sender, out int bytes);
    public abstract void SendBroadcastData (byte[] buffer, int bytes);
  }
}