//using System;
//using UdpKit;
//using UnityEngine;

///// <summary>
///// Base class for all entity serializers
///// </summary>
//public abstract class BoltEntitySerializer : BoltEntityBehaviourBase, IBoltEntityCallbacks {

//  /// <summary>
//  /// The dirty mask to use for a newly created simple proxy
//  /// </summary>
//  [Obsolete]
//  public virtual Bits proxyMask {
//    get { return Bits.all; }
//  }

//  /// <summary>
//  /// The dirty mask to use for a newly created controller proxy
//  /// </summary>
//  [Obsolete]
//  public virtual Bits controllerMask {
//    get { return Bits.all; }
//  }

//  /// <summary>
//  /// Returns the serializer itself
//  /// </summary>
//  [Obsolete("Use BoltEntitySerializer.serializer instead")]
//  public new BoltEntitySerializer boltSerializer {
//    get { return this; }
//  }

//  public new BoltEntitySerializer serializer {
//    get { return this; }
//  }

//  /// <summary>
//  /// Called when the serializer is attached to an entity
//  /// </summary>
//  public override void Attached () {}

//  /// <summary>
//  /// Called aftr all Attached() methods have been called, and after the entity has been scoped
//  /// </summary>
//  public virtual void AttachedLate () { }

//  /// <summary>
//  /// Called for packing an entity state update on the network
//  /// </summary>
//  /// <param name="info">Contains information about the target connection and frame</param>
//  /// <param name="stream">The stream to write into</param>
//  /// <param name="mask">The mask of dirty bits set</param>
//  public abstract void Pack (BoltEntityUpdateInfo info, UdpStream stream, ref Bits mask);

//  /// <summary>
//  /// Called for reading an entity state update from the network
//  /// </summary>
//  /// <param name="info">Contains information about the source connection and frame</param>
//  /// <param name="stream">The stream to read from</param>
//  public abstract void Read (BoltEntityUpdateInfo info, UdpStream stream);

//  /// <summary>
//  /// Called during the unity Update call, to update rendering of the serializer
//  /// </summary>
//  public virtual void UpdateRender () { }

//  /// <summary>
//  /// Called before the entity is stepped one simulation frame forward
//  /// </summary>
//  public virtual void BeforeStep () { }

//  /// <summary>
//  /// Called after the entity has been stepped on simulation frame
//  /// </summary>
//  public virtual void AfterStep () { }

//  /// <summary>
//  /// Called after a remote proxy has been teleported
//  /// </summary>
//  public virtual void Teleported () { }

//  /// <summary>
//  /// Called before sending a packet
//  /// </summary>
//  public virtual void BeforeSend () { }

//  /// <summary>
//  /// Called before this entitys origin changes
//  /// </summary>
//  public virtual void OriginChanging (Transform old, Transform @new) { }

//  /// <summary>
//  /// Calculates the priority of this entity in relation to the connection supplied
//  /// </summary>
//  /// <param name="cn">The connection we are calculating priority for</param>
//  /// <param name="mask">The connections dirty mask for this entity</param>
//  /// <param name="skipped">How many updates that have been skipped for this entity to the connection</param>
//  public virtual float CalculatePriority (BoltConnection cn, Bits mask, uint skipped) {
//    return skipped + 1u;
//  }

//  /// <summary>
//  /// Decides if the entity is within the scope of this connection and should be proxied to it
//  /// </summary>
//  /// <param name="cn">The connection to check scope for</param>
//  public virtual bool InScope (BoltConnection cn) {
//    return true;
//  }

//  public override string ToString () {
//    return string.Format("[Serializer type={0} entity={1}]", GetType().Name, entity);
//  }
//}
