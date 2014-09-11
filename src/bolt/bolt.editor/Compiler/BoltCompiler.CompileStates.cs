using System;
using System.Linq;
using UnityEngine;

partial class BoltCompiler {
  static void CompileStates (BoltCompilerOperation op) {
    mode = BoltCompilerMode.State;

    using (BoltSourceFile file = new BoltSourceFile(op.stateFilePath)) {
      EmitFileHeader(file);

      // configure assets
      foreach (BoltStateAsset s in op.states) {
        s.AssignBits();

        foreach (BoltAssetPropertyGroup g in s.allGroups) {
          foreach (BoltAssetProperty p in g.allProperties) {
            p.bit = g.bit;
            p.syncTarget = g.syncTarget;
            p.syncMode = BoltAssetSyncMode.Changed;
          }
        }
      }

      foreach (BoltStateAsset s in op.states) {
        // factory class
        file.EmitScope("class {0} : IBoltStateFactory", s.factoryName, () => {
          file.EmitLine("public Type stateType {{ get {{ return typeof({0}); }} }}", s.interfaceName);
          file.EmitLine("public IBoltState Create() {{ return new {0}(); }}", s.className);
        });

        // public interface
        file.EmitScope("public interface {0} : IBoltState", s.interfaceName, () => {
          // emit changed event 
          foreach (BoltAssetProperty p in s.valueProperties) {
            if (p.hasNotifyCallback) {
              file.EmitLine("Action {1}Changed {{ get; set; }}", p.type, p.name);
            }
          }

          // emit value property
          foreach (BoltAssetProperty p in s.allProperties) {
            if (p.isDefault) {
              file.EmitLine("{0} {1} {{ get; }}", p.runtimeType, p.name);
            } else {
              file.EmitLine("{0} {1} {{ get; set; }}", p.runtimeType, p.name);
            }
          }
        });

        // frame object
        file.EmitScope("class {0} : BoltStateFrame", s.frameclassName, () => {
          foreach (BoltAssetProperty p in s.valueProperties) {
            file.EmitLine("public {0} {1};", p.type, p.name);
          }

          // update method
          file.EmitScope("public override BoltStateFrame Clone ()", () => {
            file.EmitLine("{0} _clone = new {0}();", s.frameclassName);
            file.EmitLine("_clone._mask = this._mask;");
            file.EmitLine("_clone._frame = this._frame;");

            foreach (BoltAssetProperty p in s.valueProperties) {
              file.EmitLine("_clone.{0} = this.{0};", p.name);
            }

            file.EmitLine("return _clone;");
          });
        });

        // stateobject
        file.EmitScope("class {0} : BoltState<{2}>, {1}", s.className, s.interfaceName, s.frameclassName, () => {
          EmitInterfaceImplementation(file, s);

          EmitTransformClass(file, s);

          // property for create mask
          file.EmitLine("public override Bits proxyMask {{ get {{ return {0}u; }} }}", s.Mask(BoltAssetSyncTarget.Proxy));
          file.EmitLine("public override Bits controllerMask {{ get {{ return {0}u; }} }}", s.Mask(BoltAssetSyncTarget.Controller));

          EmitPackMethod(file, s);
          EmitReadMethod(file, s);

          // originchanging method
          file.EmitScope("public override void OriginChanging (UnityEngine.Transform o, UnityEngine.Transform n)", () => {
            foreach (BoltAssetProperty p in s.referenceProperties) {
              file.EmitLine("{0}.OriginChanging(o, n);", p.backingFieldName);
            }
          });

          // update method
          file.EmitScope("public override void UpdateRender ()", () => {
            foreach (BoltAssetProperty p in s.referenceProperties) {
              file.EmitLine("{0}.UpdateRender();", p.backingFieldName);
            }
          });

          // propertychanged method
          file.EmitScope("public override void PropertyChanged (IBoltStateProperty property)", () => {
            foreach (BoltAssetProperty p in s.referenceProperties) {
              if (p.syncMode == BoltAssetSyncMode.Changed) {
                file.EmitScope("if (ReferenceEquals(property, {0}))", p.backingFieldName, () => {
                  file.EmitLine("_entity.SetMaskBits(1u << {0});", p.bit);
                });
              }
            }
          });

          EmitBeforeStepMethod(file, s);

          // afterstep method
          file.EmitScope("public override void AfterStep ()", () => {
            foreach (BoltAssetProperty p in s.referenceProperties) {
              file.EmitLine("{0}.AfterStep();", p.backingFieldName);
            }
          });

          // afterstep method
          file.EmitScope("public override void Teleported ()", () => {
            foreach (BoltAssetProperty p in s.referenceProperties) {
              file.EmitLine("{0}.Teleported();", p.backingFieldName);
            }
          });

          // initialize method
          file.EmitScope("public override void Initialize ()", () => {
            if (s.tranform.enabled) {
              file.EmitLine("{0} = new TransformImpl(this, _entity);", s.tranform.backingFieldName);
            }

            if (s.mecanim.enabled && s.mecanim.mecanimSettings.mecanimAsset) {
              file.EmitLine("{0} = new {1}(this, _entity);", s.mecanim.backingFieldName, s.mecanim.mecanimSettings.mecanimAsset.name);
            }
          });
        });
      }
    }
  }

  static void EmitTransformClass (BoltSourceFile file, BoltStateAsset s) {
    if (s.tranform.enabled) {
      switch (s.tranform.transformSettings.mode) {
        case BoltAssetTransformModes.InterpolatedSnapshots:
          EmitTransformInterpolatedSnapshotsClass(file, s);
          break;

        case BoltAssetTransformModes.DeadReckoning:
          EmitTransformDeadReckoningClass(file, s);
          break;
      }
    }
  }

  static void EmitTransformInterpolatedSnapshotsClass (BoltSourceFile file, BoltStateAsset s) {
    var set = s.tranform.transformSettings;

    file.EmitScope("class TransformImpl : BoltStateTransformInterpolatedSnapshots", () => {
      file.EmitScope("public TransformImpl (IBoltState state, BoltEntity entity) : base(state, entity)", () => {
        file.EmitLine("_stepOnController = {0};", ((s.tranform.syncTarget & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller).ToString().ToLowerInvariant());
        file.EmitLine("_stepOnProxy = {0};", ((s.tranform.syncTarget & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy).ToString().ToLowerInvariant());
      });

      file.EmitScope("protected override void PackFrame (Frame f, bool pos, bool rot, UdpStream stream)", () => {
        file.EmitScope("if (pos)", () => {
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.posAxes, set.posCompression, "f.pos", true);
        });
        file.EmitScope("if (rot)", () => {
          if (set.rotAllAxes) {
            EmitVectorSerializer(file, BoltAssetPropertyType.Quaternion, set.rotAxes | BoltAssetAxes.W, set.rotCompression, "f.rot", true);
          } else {
            file.EmitLine("var rotEuler = f.rot.eulerAngles;");
            EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.rotAxes, set.rotCompression, "rotEuler", true);
          }
        });
      });

      file.EmitScope("protected override Frame ReadFrame (bool pos, bool rot, UdpStream stream)", () => {
        file.EmitLine("Frame f = new Frame();");

        file.EmitScope("if (pos)", () => {
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.posAxes, set.posCompression, "f.pos", false);
        });

        file.EmitScope("if (rot)", () => {
          if (set.rotAllAxes) {
            EmitVectorSerializer(file, BoltAssetPropertyType.Quaternion, set.rotAxes | BoltAssetAxes.W, set.rotCompression, "f.rot", false);
          } else {
            file.EmitLine("var rotEuler = Vector3.zero;");
            EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.rotAxes, set.rotCompression, "rotEuler", false);
            file.EmitLine("f.rot = Quaternion.Euler(rotEuler);");
          }
        });

        file.EmitLine("return f;");
      });
    });
  }

  static void EmitTransformDeadReckoningClass (BoltSourceFile file, BoltStateAsset s) {
    var set = s.tranform.transformSettings;

    file.EmitScope("class TransformImpl : BoltStateTransformDeadReckoning", () => {
      file.EmitScope("public TransformImpl (IBoltState state, BoltEntity entity) : base(state, entity)", () => {
        file.EmitLine("_inferVelocity = {0};", set.inferVelocity.ToString().ToLowerInvariant());
        file.EmitLine("_inferAcceleration = {0};", set.inferAcceleration.ToString().ToLowerInvariant());
        file.EmitLine("_toleranceVelocity = {0}f;", set.velZeroTolerance.ToString());
        file.EmitLine("_toleranceAcceleration = {0}f;", set.accZeroTolerance.ToString());
        file.EmitLine("_useAcceleration = {0};", set.useAcceleration.ToString().ToLowerInvariant());
        file.EmitLine("_zeroVelocityX = {0};", ((set.velAxes & BoltAssetAxes.X) != BoltAssetAxes.X).ToString().ToLowerInvariant());
        file.EmitLine("_zeroVelocityY = {0};", ((set.velAxes & BoltAssetAxes.Y) != BoltAssetAxes.Y).ToString().ToLowerInvariant());
        file.EmitLine("_zeroVelocityZ = {0};", ((set.velAxes & BoltAssetAxes.Z) != BoltAssetAxes.Z).ToString().ToLowerInvariant());
        file.EmitLine("_maxForwardExtrap = {0};", set.maxForwardExtrapolation);
        file.EmitLine("_maxInterpTime = {0};", set.maxInterpTime);
        file.EmitLine("_stepOnController = {0};", ((s.tranform.syncTarget & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller).ToString().ToLowerInvariant());
        file.EmitLine("_stepOnProxy = {0};", ((s.tranform.syncTarget & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy).ToString().ToLowerInvariant());
      });

      file.EmitScope("public override void PackFrame (BoltEntityUpdateInfo info, UdpStream stream)", () => {
        EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.posAxes, set.posCompression, "_owner.position", true);

        if (set.inferVelocity == false) {
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.velAxes, set.velCompression, "_owner.velocity", true);
        }

        if (set.useAcceleration && set.inferAcceleration == false) {
          EmitFloatSerializer(file, set.accCompression, "_owner.acceleration", true);
        }

        if (set.rotAllAxes) {
          EmitVectorSerializer(file, BoltAssetPropertyType.Quaternion, set.rotAxes | BoltAssetAxes.W, set.rotCompression, "_owner.rotation", true);
        } else {
          file.EmitLine("var rotEuler = _owner.rotation.eulerAngles;");
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.rotAxes, set.rotCompression, "rotEuler", true);
        }
      });

      file.EmitScope("public override void ReadFrame (BoltEntityUpdateInfo info, UdpStream stream)", () => {
        file.EmitLine("Frame f = default(Frame);");
        file.EmitLine("f.frame = info.frame;");

        EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.posAxes, set.posCompression, "f.position", false);

        if (set.inferVelocity == false) {
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.velAxes, set.velCompression, "f.velocity", false);
        }

        if (set.useAcceleration && set.inferAcceleration == false) {
          EmitFloatSerializer(file, set.accCompression, "f.acceleration", false);
        }

        if (set.rotAllAxes) {
          EmitVectorSerializer(file, BoltAssetPropertyType.Quaternion, set.rotAxes | BoltAssetAxes.W, set.rotCompression, "f.rotation", false);
        } else {
          file.EmitLine("var rotEuler = Vector3.zero;");
          EmitVectorSerializer(file, BoltAssetPropertyType.Vector3, set.rotAxes, set.rotCompression, "rotEuler", false);
          file.EmitLine("f.rotation = Quaternion.Euler(rotEuler);");
        }

        file.EmitLine("_buffer.Enqueue(f);");
      });
    });
  }

  static void EmitInterfaceImplementation (BoltSourceFile file, BoltStateAsset s) {
    // emit all backing fields
    foreach (BoltAssetProperty p in s.allProperties) {
      file.EmitLine("{0} {1};", p.runtimeType, p.backingFieldName);

      if (p.smoothed) {
        file.EmitLine("{1} {0}actual;", p.backingFieldName, p.runtimeType);
        file.EmitLine("int {0}frame;", p.backingFieldName);
      }
    }

    // emit all change events
    foreach (BoltAssetProperty p in s.valueProperties) {
      if (p.hasNotifyCallback) {
        file.EmitLine("Action {2}.{1}Changed {{ get; set; }}", p.runtimeType, p.name, s.interfaceName);
      }
    }

    // emit property implementations
    foreach (BoltAssetProperty p in s.allProperties) {
      if (p.isDefault) {
        file.EmitScope("{0} {2}.{1}", p.runtimeType, p.name, s.interfaceName, () => {
          file.EmitLine("get {{ return {0}; }}", p.backingFieldName);
        });

      } else {
        file.EmitScope("{0} {2}.{1}", p.type, p.name, s.interfaceName, () => {
          file.EmitScope("get", () => {
            file.EmitLine("return {0};", p.backingFieldName);
          });

          if (p.type == BoltAssetPropertyType.Custom) {
            file.EmitScope("set", () => {
              file.EmitLine("if ({0} != null) throw new BoltException(\"Can't change custom property after setting it\");", p.backingFieldName);
              file.EmitLine("if (value == null) throw new BoltException(\"Can't assign null to a custom property\");");
              file.EmitLine("{0} = value;", p.backingFieldName);
            });
          }
          else {
            file.EmitScope("set", () => {
              bool sendToProxy = (p.syncTarget & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy;
              bool sendToController = (p.syncTarget & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller;

              file.EmitScope("if (_entity.boltIsOwner == false)", () => {
                Action exception = () => file.EmitLine("throw new BoltException(\"you are not allowed to set {0}.{1} locally\");", s.interfaceName, p.name);

                if (sendToProxy && sendToController) {
                  exception();
                }
                else {
                  if (sendToProxy) {
                    file.EmitScope("if (_entity.boltIsControlling == false)", exception);
                  }

                  if (sendToController) {
                    file.EmitScope("if (_entity.boltIsControlling)", exception);
                  }
                }
              });

              file.EmitScope("if ({0} != value)", p.backingFieldName, () => {
                file.EmitLine("{0} = value;", p.backingFieldName);

                if (p.bit != int.MaxValue) {
                  file.EmitLine("_entity.SetMaskBits(1u << {0});", p.bit);
                }

                if (p.hasNotifyCallback) {
                  file.EmitLine("TriggerChangedEvent((({1})this).{0}Changed);", p.name, s.interfaceName);
                }
              });
            });
          }
        });
      }
    }
  }

  static void EmitPackMethod (BoltSourceFile file, BoltStateAsset s) {
    file.EmitScope("public override bool Pack (BoltEntityUpdateInfo info, UdpStream stream, ref Bits mask)", () => {
      file.EmitLine("bool isController = ReferenceEquals(_entity._remoteController, info.connection);");
      file.EmitLine("bool isProxy = !isController;");
      file.EmitLine("bool check = false;");

      // this we only send once (on init)
      file.EmitScope("if (info.first)", () => {
        foreach (BoltAssetProperty p in s.globalProperties.Where(x => x.syncMode == BoltAssetSyncMode.OnceOnInit)) {
          EmitProxyControllerCheck(file, p.syncTarget, () => {
            EmitWrite(file, p, p.backingFieldName, "info.connection");
          });
        }
      });

      // this we only send if it has changed
      foreach (BoltAssetProperty p in s.globalProperties.Where(x => x.syncMode == BoltAssetSyncMode.Changed)) {
        EmitProxyControllerCheck(file, p.syncTarget, () => {

          if (p.type == BoltAssetPropertyType.Entity) {
            file.EmitLine("check = " + string.Format("(this.{0} == null || info.connection._entityChannel.ExistsOnRemote(this.{0}));", p.backingFieldName));
            file.EmitScope("if (stream.WriteBool(check && (mask & (1u << {0}))))", p.bit, () => {
              EmitWrite(file, p, p.backingFieldName, "info.connection");
              file.EmitLine("mask &= ~(1u << {0});", p.bit);
            });

          } else {
            EmitBitCheckedWrite(file, p.bit, () => {
              EmitWrite(file, p, p.backingFieldName, "info.connection");
            });
          }
        });
      }

      // groups are only sent if they have changed
      foreach (BoltAssetPropertyGroup g in s.allGroups) {
        EmitProxyControllerCheck(file, g.syncTarget, () => {
          EmitBitCheckedWrite(file, g.bit, () => {
            foreach (BoltAssetProperty p in g.allProperties) {
              EmitWrite(file, p, p.backingFieldName, "info.connection");
            }
          });
        });
      }

      file.EmitLine("return true;");
    });
  }

  static void EmitReadMethod (BoltSourceFile file, BoltStateAsset s) {
    file.EmitScope("public override void Read (BoltEntityUpdateInfo info, UdpStream stream)", () => {
      file.EmitLine("bool isController = _entity.boltIsControlling;");
      file.EmitLine("bool isProxy = !isController;");
      file.EmitLine("_recv._mask = 0u;");
      file.EmitLine("_recv._frame = info.frame;");

      // this we only read once (on init)
      file.EmitScope("if (info.first)", () => {
        foreach (BoltAssetProperty p in s.globalProperties.Where(x => x.syncMode == BoltAssetSyncMode.OnceOnInit)) {
          EmitProxyControllerCheck(file, p.syncTarget, () => {
            EmitRead(file, p);
          });
        }
      });

      // this we only send if it has changed
      foreach (BoltAssetProperty p in s.globalProperties.Where(x => x.syncMode == BoltAssetSyncMode.Changed)) {
        EmitProxyControllerCheck(file, p.syncTarget, () => {
          if (p.type == BoltAssetPropertyType.Transform) {
            EmitBitCheckedRead(file, p.bit, () => {
              EmitRead(file, p);
            }, () => {
              file.EmitLine("{0}.Skip(info);", p.backingFieldName);
            });
          } else {
            EmitBitCheckedRead(file, p.bit, () => {
              EmitRead(file, p);
            }, null);
          }
        });
      }

      // groups are only sent if they have changed
      foreach (BoltAssetPropertyGroup g in s.allGroups) {
        EmitProxyControllerCheck(file, g.syncTarget, () => {
          EmitBitCheckedRead(file, g.bit, () => {
            foreach (BoltAssetProperty p in g.allProperties) {
              EmitRead(file, p);
            }
          }, null);
        });
      }

      file.EmitLine("_buffer.AddLast(({0}) _recv.Clone());", s.frameclassName);
    });
  }

  static void EmitBitCheckedWrite (BoltSourceFile file, int bit, Action action) {
    file.EmitScope("if (stream.WriteBool(mask & (1u << {0})))", bit, () => {
      action();

      // clear this flag
      file.EmitLine("mask &= ~(1u << {0});", bit);
    });
  }

  static void EmitBitCheckedRead (BoltSourceFile file, int bit, Action action, Action elseAction) {
    file.EmitScope("if (stream.ReadBool())", bit, () => {
      action();

      file.EmitLine("_recv._mask |= (1u << {0});", bit);
    });

    if (elseAction != null) {
      file.EmitScope("else", elseAction);
    }
  }

  static void EmitRead (BoltSourceFile file, BoltAssetProperty p) {
    if (p.isReference) {
      EmitRead(file, p, p.backingFieldName, "info.connection");
    } else {
      EmitRead(file, p, "_recv.{0}", "info.connection");
    }
  }

  static void EmitChangedEventTrigger (BoltSourceFile file, BoltStateAsset s, BoltAssetProperty p, string oldExpr, string newExpr, bool emitCheck) {
    if (p.hasNotifyCallback == false) {
      return;
    }

    string cmp;

    switch (p.type) {
      case BoltAssetPropertyType.Custom:
      case BoltAssetPropertyType.Transform:
      case BoltAssetPropertyType.Mecanim:
        return;

      case BoltAssetPropertyType.String:
        cmp = "if (BoltUtils.StringEquals({0}, {1}) == false)";
        break;

      case BoltAssetPropertyType.Entity:
        cmp = "if (ReferenceEquals({0}, {1}) == false)";
        break;

      default:
        cmp = "if ({0}.Equals({1}) == false)";
        break;
    }

    cmp = string.Format(cmp, oldExpr, newExpr);

    Action trigger = () => {
      file.EmitScope("try", () => {
        file.EmitLine("TriggerChangedEvent<{1}>((this as {2}).{0}Changed, " + oldExpr + ", " + newExpr + ");", p.name, p.type, s.interfaceName);
      });

      file.EmitScope("catch (Exception exn)", () => {
        file.EmitLine("BoltLog.Exception(exn);");
      });
    };

    if (emitCheck) {
      file.EmitScope(cmp, p.name, trigger);
    } else {
      trigger();
    }
  }

  static void EmitBeforeStepMethod (BoltSourceFile file, BoltStateAsset s) {
    file.EmitScope("public override void BeforeStep ()", () => {
      file.EmitScope("if (_entity.boltIsProxy)", () => {
        file.EmitScope("while (_buffer.count > 0 && _buffer.first._frame <= _entity.boltFrame)", () => {
          file.EmitLine("{0} f = _buffer.RemoveFirst();", s.frameclassName);
          file.EmitLine("_entity.SetMaskBits(f._mask);");

          foreach (BoltAssetProperty p in s.valueProperties.Where(x => x.hasNotifyCallback)) {
            file.EmitLine("bool {0}hasChanged = false;", p.backingFieldName);
          }

          file.EmitScope("if (_entity.boltIsControlling)", () => {
            foreach (BoltAssetProperty p in s.valueProperties.Where(x => (x.syncTarget & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller)) {
              EmitPropertyBeforeStep(file, p);
            }
          });

          file.EmitScope("else", () => {
            foreach (BoltAssetProperty p in s.valueProperties.Where(x => (x.syncTarget & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy)) {
              EmitPropertyBeforeStep(file, p);
            }
          });

          file.EmitScope("try", () => {
            foreach (BoltAssetProperty p in s.valueProperties.Where(x => x.hasNotifyCallback)) {
              file.EmitScope("if ({0}hasChanged)", p.backingFieldName, () => {
                file.EmitLine("TriggerChangedEvent((({1})this).{0}Changed);", p.name, s.interfaceName);
              });
            }
          });

          file.EmitLine("catch (Exception exn) { BoltLog.Exception(exn); }");

          file.EmitLine("f.Dispose();");
        });

        file.EmitScope("if (_buffer.count > 0)", () => {
          file.EmitLine("{0} f = _buffer.first;", s.frameclassName);

          // properties smoothed on controller
          file.EmitScope("if (_entity.boltIsControlling)", () => {
            foreach (BoltAssetProperty p in s.valueProperties.Where(x => (x.syncTarget & BoltAssetSyncTarget.Controller) == BoltAssetSyncTarget.Controller).Where(x => x.smoothed)) {
              EmitPropertySmoother(file, p);
            }
          });

          // properties smoothed on proxy
          file.EmitScope("else", () => {
            foreach (BoltAssetProperty p in s.valueProperties.Where(x => (x.syncTarget & BoltAssetSyncTarget.Proxy) == BoltAssetSyncTarget.Proxy).Where(x => x.smoothed)) {
              EmitPropertySmoother(file, p);
            }
          });
        });

      });

      foreach (BoltAssetProperty p in s.referenceProperties) {
        file.EmitLine("{0}.BeforeStep();", p.backingFieldName);
      }
    });
  }

  static void EmitPropertySmoother (BoltSourceFile file, BoltAssetProperty p) {
    file.EmitLine("{1} = FrameSmoothed({1}actual, f.{0}, {1}frame, f._frame, _entity.boltFrame);", p.name, p.backingFieldName);
  }

  static void EmitPropertyBeforeStep (BoltSourceFile file, BoltAssetProperty p) {
    if (p.hasNotifyCallback) {
      file.EmitLine("{0}hasChanged = {0} != f.{1};", p.backingFieldName, p.name);
    }

    file.EmitLine("{0} = f.{1};", p.backingFieldName, p.name);

    if (p.smoothed) {
      file.EmitLine("{0}actual = f.{1};", p.backingFieldName, p.name);
      file.EmitLine("{0}frame = f._frame;", p.backingFieldName);
    }
  }
}
