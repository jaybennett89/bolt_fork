using System;
using UdpKit;

namespace Bolt {
  /// <summary>
  /// Base class that all events inherit from
  /// </summary>
  /// <example>
  /// *Example:* Using a LogEvent object to send a message.
  /// 
  /// ```csharp
  /// void LogMessage(string message) { 
  ///   var logEvt = new LogEvent();
  ///   logEvt.message = message;
  ///   logEvt.Send();
  /// }
  /// ```
  /// </example>
  [Documentation]
  public abstract class Event : NetworkObj_Root {
    internal const byte ENTITY_EVERYONE = 1;
    internal const byte ENTITY_EVERYONE_EXCEPT_OWNER = 3;
    internal const byte ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER = 13;
    internal const byte ENTITY_EVERYONE_EXCEPT_CONTROLLER = 5;
    internal const byte ENTITY_ONLY_CONTROLLER = 7;
    internal const byte ENTITY_ONLY_CONTROLLER_AND_OWNER = 15;
    internal const byte ENTITY_ONLY_OWNER = 9;
    internal const byte ENTITY_ONLY_SELF = 11;

    internal const byte GLOBAL_EVERYONE = 2;
    internal const byte GLOBAL_OTHERS = 4;
    internal const byte GLOBAL_ONLY_SERVER = 6;
    internal const byte GLOBAL_ALL_CLIENTS = 8;
    internal const byte GLOBAL_SPECIFIC_CONNECTION = 10;
    internal const byte GLOBAL_ONLY_SELF = 12;

    internal const int RELIABLE_WINDOW_BITS = 10;
    internal const int RELIABLE_SEQUENCE_BITS = 12;

    NetworkStorage storage;

    internal uint Sequence;
    internal ReliabilityModes Reliability;

    internal int Targets;
    internal bool Reliable;
    internal Entity TargetEntity;
    internal BoltConnection TargetConnection;
    internal BoltConnection SourceConnection;

    internal new Event_Meta Meta;

    /// <summary>
    /// Returns true if this event was sent from own connection
    /// </summary>
    /// <example>
    /// *Example:* Showing chat messages from other players.
    /// 
    /// ```csharp
    /// public override void OnEvent(ChatEvent chatEvt) {
    ///   if(chatEvt.FromSelf) {
    ///     return;
    ///   }
    ///   
    ///   ChatWindow.instance.ShowMessage(chatEvt.message, chatEvt.timestamp);
    /// }
    /// ```
    /// </example> 
    public bool FromSelf {
      get { return ReferenceEquals(SourceConnection, null); }
    }

    /// <summary>
    /// The connection which raised this event
    /// </summary>
    /// <example>
    /// *Example:* Blocking messages from players on a chat restricted list.
    /// 
    /// ```csharp
    /// public override void OnEvent(ChatEvent chatEvt) {
    ///   if(chatRestrictedPlayerList.ContainsKey(chatEvt.RaisedBy)) {
    ///     return;
    ///   }
    ///   
    ///   ChatWindow.instance.ShowMessage(chatEvt.message, chatEvt.timestamp);
    /// }
    /// ```
    /// </example>
    public BoltConnection RaisedBy {
      get { return SourceConnection; }
    }

    /// <summary>
    /// Returns true if this is a global event / not an entity event
    /// </summary>
    /// <example>
    /// *Example:* Using the isGlobal property to determine whether to send local or whole-zone chat.
    /// 
    /// ```csharp
    /// public override void OnEvent(ChatEvent chatEvt) {
    ///   if(chatEvt.isGlobalEvent) {
    ///     BroadcastZoneChat(chatEvt.message, chatEvt.timestamp);
    ///   }
    ///   else {
    ///     SendLocalChat(chatEvt.message, chatEvt.timestamp);
    ///   }   
    /// }
    /// ```
    /// </example>
    public bool IsGlobalEvent {
      get { return !IsEntityEvent; }
    }

    internal override NetworkStorage Storage {
      get { return storage; }
    }

    /// <summary>
    /// The raw bytes of the event data
    /// </summary>
    /// <example>
    /// *Example:* Removing repeated chat messages by doing sequence comparison on the raw byte data and filtering out 
    /// any repeated messages after a certain limit.
    /// 
    /// ```csharp
    /// public override void OnEvent(ChatEvent chatEvt) {
    ///   bool repeated = true;
    ///   
    ///   for(int i = 0; i < CHAT_REPEAT_LIMIT; i++) {
    ///     if(!chatEvt.BinaryData.SequenceEqual(previousChatEvts.GoBack(i).BinaryData) {
    ///       repeated = false;
    ///       break;
    ///     }
    ///   }
    ///   
    ///   
    /// }
    /// ```
    /// </example>
    public byte[] BinaryData {
      get;
      set;
    }

    internal bool IsEntityEvent {
      get {
        return
          Targets == ENTITY_EVERYONE ||
          Targets == ENTITY_EVERYONE_EXCEPT_OWNER ||
          Targets == ENTITY_EVERYONE_EXCEPT_CONTROLLER ||
          Targets == ENTITY_EVERYONE_EXCEPT_OWNER_AND_CONTROLLER ||
          Targets == ENTITY_ONLY_CONTROLLER ||
          Targets == ENTITY_ONLY_SELF ||
          Targets == ENTITY_ONLY_CONTROLLER_AND_OWNER ||
          Targets == ENTITY_ONLY_OWNER;
      }
    }

    internal Event(Event_Meta meta)
      : base(meta) {
      Meta = meta;
      storage = AllocateStorage();
    }

    internal void FreeStorage() {
      if (storage != null) {
        Meta.FreeStorage(storage);
      }
    }

    internal bool Pack(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        if (Meta.Properties[i].Property.Write(connection, this, storage, packet) == false) {
          return false;
        }
      }

      return true;
    }

    internal void Read(BoltConnection connection, UdpPacket packet) {
      for (int i = 0; i < Meta.Properties.Length; ++i) {
        Meta.Properties[i].Property.Read(connection, this, storage, packet);
      }
    }

    /// <summary>
    /// Enqueue this object for sending across the network
    /// </summary>
    /// <example>
    /// *Example:* Sending a log message.
    /// 
    /// ```csharp
    /// void LogMessage(string message) { 
    ///   var logEvt = new LogEvent();
    ///   logEvt.message = message;
    ///   logEvt.Send();
    /// }
    /// ```
    /// </example>
    public void Send() {
      EventDispatcher.Enqueue(this);
    }
  }
}
