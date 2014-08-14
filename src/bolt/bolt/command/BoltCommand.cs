using System;
using UdpKit;

/// <summary>
/// Base class for all commands
/// </summary>
public abstract class BoltCommand : BoltObject, IDisposable {

  internal const uint FLAG_HAS_EXECUTED = 1;
  internal const uint FLAG_STATE_SENT = 2;
  internal const uint FLAG_STATE_RECEIVED = 4;
  internal const uint FLAG_STATE_DISPOSE = 8;

  internal readonly ushort _id;

  internal int _serverFrame;
  internal Bits _flags;

  internal ushort _sequence;

  internal bool _hasExecuted;
  internal bool _stateSent;
  internal bool _stateRecv;
  internal bool _dispose;

  /// <summary>
  /// The estimated server frame on the host that this command originated from.
  /// If this command was created on the server, this will be the same as sourceFrame.
  /// </summary>
  public int serverFrame {
    get { return _serverFrame; }
  }

  /// <summary>
  /// Returns true if this is the first execution of the command
  /// </summary>
  public bool isFirstExecution {
    get { return !_hasExecuted; }
  }

  /// <summary>
  /// Returns true if we have received the corrected state from the server
  /// </summary>
  public bool stateReceived {
    get { return _stateRecv; }
  }

  protected BoltCommand (ushort id) {
    _id = id;
  }

  /// <summary>
  /// Clones the current command
  /// </summary>
  public virtual BoltCommand Clone () {
    BoltCommand cmd = (BoltCommand) MemberwiseClone();
    IBoltListNode node = (IBoltListNode) cmd;
    node.prev = null;
    node.next = null;
    node.list = null;
    return cmd;
  }

  public override string ToString () {
    return string.Format("[{0} sequence={1}]", GetType().Name, _sequence);
  }

  public static implicit operator bool (BoltCommand cmd) {
    return cmd != null;
  }

  /// <summary>
  /// Called when a command is disposed
  /// </summary>
  public abstract void Dispose ();

  /// <summary>
  /// Called each frame to interpolate from a local incorrect state to a 
  /// remote correct state.
  /// </summary>
  public abstract void Interpolate ();

  /// <summary>
  /// Called on the server for sending the state of a command to the client
  /// </summary>
  /// <param name="connection">The connection we are sending to</param>
  /// <param name="stream">The packet stream</param>
  public abstract void PackState (BoltConnection connection, UdpStream stream);

  /// <summary>
  /// Called on the client for reading the state of a command from the server
  /// </summary>
  /// <param name="connection">The connection we received the state from</param>
  /// <param name="stream">The packet stream</param>
  public abstract void ReadState (BoltConnection connection, UdpStream stream);

  /// <summary>
  /// Called on the client for sending a command to the server
  /// </summary>
  /// <param name="connection">The connection we are sending to</param>
  /// <param name="stream">The packet stream</param>
  public abstract void PackInput (BoltConnection connection, UdpStream stream);

  /// <summary>
  /// Called on the server for reading a command from the client
  /// </summary>
  /// <param name="connection">The connection we received the command from</param>
  /// <param name="stream">The packet stream</param>
  public abstract void ReadInput (BoltConnection connection, UdpStream stream);
}
