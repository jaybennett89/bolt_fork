//using System.Collections.Generic;
//using System.Linq;

//partial class BoltCompiler {
//  static void EmitOnEventMethods(BoltSourceFile file, IEnumerable<BoltEventAsset> events, bool isGlobal) {
//    foreach (BoltEventAsset evnt in events.Where(x => (x.eventMode == BoltAssetEventMode.Global) == isGlobal)) {
//      file.EmitLine("public virtual void OnEvent({1} evnt, BoltConnection cn) {{ }}", evnt.name, evnt.interfaceName);
//    }
//  }

//  static string GetInterfaces(IEnumerable<BoltEventAsset> events, bool isGlobal) {
//    string interfaces = "";

//    events = events.Where(x => (x.eventMode == BoltAssetEventMode.Global) == isGlobal);

//    if (events.Count() > 0) {
//      interfaces = ", " + events.Select(x => x.receiverName).Join(", ");
//    }

//    return interfaces;
//  }

//  static string TypeName(BoltAssetPropertyType type) {
//    switch (type) {
//      case BoltAssetPropertyType.ByteArray: return "byte[]";
//    }

//    return type.ToString();
//  }

//  static void CompileEvents(BoltCompilerOperation op) {
//    mode = BoltCompilerMode.Event;

//    int idCounter = 0;

//    foreach (BoltEventAsset evnt in op.events) {
//      evnt.id = (ushort)(op.eventIdOffset + idCounter);
//      idCounter += 1;
//    }

//    using (BoltSourceFile file = new BoltSourceFile(op.eventFilePath)) {
//      EmitFileHeader(file);

//      return;

//      // TODO

//      file.EmitScope("public abstract class BoltEvent : BoltEventBase", () => {
//        file.EmitLine("protected BoltEvent(ushort id, bool entity, BoltEventDeliveryMode mode) : base(id, entity, mode) { }");
//      });

//      file.EmitScope("public abstract class BoltCallbacks : BoltCallbacksBase {0}", GetInterfaces(op.events, true), () => {
//        EmitOnEventMethods(file, op.events, true);
//      });

//      file.EmitScope("public abstract class BoltEntityBehaviour : BoltEntityBehaviourBase {0}", GetInterfaces(op.events, false), () => {
//        EmitOnEventMethods(file, op.events, false);
//      });

//      file.EmitScope("public abstract class BoltEntityBehaviour<TState> : BoltEntityBehaviour where TState : class, IBoltState", () => {
//        file.EmitLine("[System.Obsolete(\"Use BoltEntityBehaviour<TState>.state instead \")]");
//        file.EmitLine("public new TState boltState { get { return state; } }");
//        file.EmitLine("public new TState state { get { return ((BoltEntitySerializer<TState>) base.serializer).state; } }");
//      });

//      file.EmitLine("[System.Obsolete(\"Inherit from the BoltEntityBehaviour<TState> class instead\")]");
//      file.EmitScope("public abstract class BoltEntityBehaviour<TSerializer, TState> : BoltEntityBehaviour<TState> where TState : class, IBoltState", () => { });

//      foreach (BoltEventAsset evnt in op.events) {
//        // event interface
//        file.EmitScope("public interface {0} : IBoltEvent", evnt.interfaceName, () => {
//          foreach (BoltAssetProperty p in evnt.properties) {
//            file.EmitLine("{0} {1} {{ get; set; }}", TypeName(p.type), p.name);
//          }
//        });

//        // receiver interface
//        file.EmitScope("public interface {0}", evnt.receiverName, () => {
//          file.EmitLine("void OnEvent ({0} evnt, BoltConnection connection);", evnt.interfaceName);
//        });

//        // event class
//        file.EmitScope("class {0} : BoltEvent, {1}", evnt.name, evnt.interfaceName, () => {
//          foreach (BoltAssetProperty p in evnt.properties) {
//            file.EmitLine("{0} {1};", TypeName(p.type), p.backingFieldName);
//            file.EmitLine("public {0} {1} {{ get {{ return {2}; }} set {{ {2}  = value; }} }}", TypeName(p.type), p.name, p.backingFieldName);
//          }

//          // constructor
//          string isEntityEvent = (evnt.eventMode == BoltAssetEventMode.Entity).ToString().ToLowerInvariant();
//          string deliveryMode = evnt.deliveryMode.ToString();
//          file.EmitLine("internal {0} () : base({1}, {2}, BoltEventDeliveryMode.{3}) {{ }}", evnt.name, evnt.id, isEntityEvent, deliveryMode);

//          // filterinvoke method
//          EmitFilterInvoke(file, evnt);
//          EmitFilterSend(file, evnt);

//          // pack method
//          file.EmitScope("public override void Pack (UdpStream stream, BoltConnection cn)", () => {
//            if (evnt.id == ushort.MaxValue) {
//              file.EmitLine("throw new NotSupportedException();");
//            }
//            else {
//              foreach (BoltAssetProperty p in evnt.properties) {
//                EmitWrite(file, p, p.backingFieldName, "cn");
//              }
//            }
//          });

//          // read method
//          file.EmitScope("public override void Read (UdpStream stream, BoltConnection cn)", () => {
//            if (evnt.id == ushort.MaxValue) {
//              file.EmitLine("throw new NotSupportedException();");
//            }
//            else {
//              foreach (BoltAssetProperty p in evnt.properties) {
//                EmitRead(file, p, p.backingFieldName, "cn");
//              }
//            }
//          });

//          // dipose method
//          file.EmitScope("public override void Free ()", () => {
//            foreach (BoltAssetProperty p in evnt.properties) {
//              file.EmitLine("{0} = default({1});", p.backingFieldName, TypeName(p.type));
//            }
//          });
//        });

//        // event dispatcher
//        file.EmitScope("class {0} : IBoltEventFactory", evnt.factoryName, evnt.name, () => {

//          file.EmitLine("public Type eventType {{ get {{ return typeof({0}); }} }}", evnt.interfaceName);
//          file.EmitLine("public ushort eventId {{ get {{ return {0}; }} }}", evnt.id);

//          file.EmitScope("public object Create ()", () => {
//            file.EmitLine("return new {0}();", evnt.name);
//          });

//          file.EmitScope("public void Dispatch (object @event, object target)", () => {
//            file.EmitLine("{0} evnt = ({0})@event;", evnt.name);
//            file.EmitLine("{0} receiver = target as {0};", evnt.receiverName);
//            file.EmitScope("if (receiver != null)", () => {
//              file.EmitLine("receiver.OnEvent(evnt, evnt._connection);");
//            });
//          });
//        });
//      }
//    }
//  }

//  static void EmitFilterSend(BoltSourceFile file, BoltEventAsset evnt) {
//    file.EmitScope("public override bool FilterSend (BoltConnection cn)", () => {
//      file.EmitLine("if (ReferenceEquals(_connection, cn)) return false;");

//      switch (evnt.eventMode) {
//        case BoltAssetEventMode.Entity:
//          // if entity doesnt exist anymore dont send this event
//          file.EmitScope("if (_entity)", () => {

//            // we are the owner of this (logic for server and client is the same here)
//            file.EmitScope("if (_entity.boltIsOwner)", () => {

//              // if entity might exist on remote
//              file.EmitScope("if (cn._entityChannel.MightExistOnRemote(_entity))", () => {

//                // if we are sending to controller (this can only be true on server, but doesn't matter)
//                file.EmitScope("if (ReferenceEquals(_entity._remoteController, cn))", () => {
//                  if ((evnt.entityTarget & BoltAssetEventEntityTarget.Controller) == BoltAssetEventEntityTarget.Controller) {
//                    file.EmitLine("return true;");
//                  }
//                });

//                // if we are sending to proxies
//                file.EmitScope("else", () => {
//                  if ((evnt.entityTarget & BoltAssetEventEntityTarget.Proxy) == BoltAssetEventEntityTarget.Proxy) {
//                    file.EmitLine("return true;");
//                  }
//                });

//              });
//            });

//            // we are NOT the owner of this (logic for server and client differs)
//            file.EmitScope("else", () => {

//              // server logic
//              file.EmitScope("if (BoltCore.isServer)", () => {

//                // if this connection is the owner
//                file.EmitScope("if (ReferenceEquals(_entity._source, cn))", () => {
//                  if ((evnt.entityTarget & BoltAssetEventEntityTarget.Owner) == BoltAssetEventEntityTarget.Owner) {
//                    file.EmitLine("return true;");
//                  }
//                  else if ((evnt.entityTarget & BoltAssetEventEntityTarget.Controller) == BoltAssetEventEntityTarget.Controller) {
//                    file.EmitLine("return true;");
//                  }
//                });

//                // if we are sending to proxies
//                file.EmitScope("else", () => {
//                  if ((evnt.entityTarget & BoltAssetEventEntityTarget.Proxy) == BoltAssetEventEntityTarget.Proxy) {
//                    file.EmitLine("return true;");
//                  }
//                });

//              });

//              // client logic
//              file.EmitScope("else", () => {
//                file.EmitScope("if (_entity.boltIsControlling)", () => {
//                  if ((evnt.entitySource & BoltAssetEventEntitySource.Controller) == BoltAssetEventEntitySource.Controller) {
//                    file.EmitLine("return true;");
//                  }
//                });

//                file.EmitScope("else", () => {
//                  if ((evnt.entitySource & BoltAssetEventEntitySource.Proxy) == BoltAssetEventEntitySource.Proxy) {
//                    file.EmitLine("return true;");
//                  }
//                });
//              });

//            });
//          });
//          break;

//        case BoltAssetEventMode.Global:
//          if ((evnt.globalTarget & BoltAssetEventGlobalTarget.Client) == BoltAssetEventGlobalTarget.Client) {
//            file.EmitLine("if (cn.udpConnection.IsServer) { return true; }");
//          }

//          if ((evnt.globalTarget & BoltAssetEventGlobalTarget.Server) == BoltAssetEventGlobalTarget.Server) {
//            file.EmitLine("if (cn.udpConnection.IsClient) { return true; }");
//          }
//          break;
//      }

//      file.EmitLine("return false;");
//    });
//  }

//  static void EmitFilterInvoke(BoltSourceFile file, BoltEventAsset evnt) {
//    file.EmitScope("public override bool FilterInvoke ()", () => {
//      switch (evnt.eventMode) {
//        case BoltAssetEventMode.Entity:
//          file.EmitScope("if (_entity)", () => {

//            file.EmitScope("if (_entity.boltIsOwner)", () => {
//              if ((evnt.entityTarget & BoltAssetEventEntityTarget.Owner) == BoltAssetEventEntityTarget.Owner) {
//                file.EmitLine("return true;");
//              }
//              else {
//                if ((evnt.entityTarget & BoltAssetEventEntityTarget.Controller) == BoltAssetEventEntityTarget.Controller) {
//                  file.EmitLine("return _entity.boltIsControlling;");
//                }
//              }
//            });

//            file.EmitScope("else", () => {
//              file.EmitScope("if (_entity.boltIsControlling)", () => {
//                if ((evnt.entityTarget & BoltAssetEventEntityTarget.Controller) == BoltAssetEventEntityTarget.Controller) {
//                  file.EmitLine("return true;");
//                }
//              });

//              file.EmitScope("else", () => {
//                if ((evnt.entityTarget & BoltAssetEventEntityTarget.Proxy) == BoltAssetEventEntityTarget.Proxy) {
//                  file.EmitLine("return true;");
//                }
//              });
//            });

//          });
//          break;

//        case BoltAssetEventMode.Global:
//          file.EmitScope("if (ReferenceEquals(_connection, null))", () => {
//            if ((evnt.globalTarget & BoltAssetEventGlobalTarget.Sender) == BoltAssetEventGlobalTarget.Sender) {
//              file.EmitLine("return true;");
//            }
//          });

//          file.EmitScope("else", () => {
//            if ((evnt.globalTarget & BoltAssetEventGlobalTarget.Server) == BoltAssetEventGlobalTarget.Server)
//              file.EmitLine(" if (BoltCore.isServer) { return true; }");

//            if ((evnt.globalTarget & BoltAssetEventGlobalTarget.Client) == BoltAssetEventGlobalTarget.Client)
//              file.EmitLine(" if (BoltCore.isClient) { return true; }");
//          });

//          break;
//      }

//      file.EmitLine("return false;");
//    });
//  }
//}
