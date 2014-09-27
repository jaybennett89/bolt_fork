//using System.Collections.Generic;
//using System.Linq;
//using System.Globalization;
//using UnityEditorInternal;
//using UnityEngine;

//partial class BoltCompiler {
//  static void CompileMecanim (BoltCompilerOperation op) {
//    mode = BoltCompilerMode.Mecanim;

//    using (BoltSourceFile file = new BoltSourceFile(op.mecanimFilePath)) {
//      EmitFileHeader(file);

//      foreach (BoltMecanimAsset m in op.mecanims) {
//        if (!m.controller) {
//          continue;
//        }

//        BoltEditorUtils.SynchronizeWithController(m);

//        file.EmitScope("public interface {0} : IBoltStateProperty", m.interfaceName, () => {
//          foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//            file.EmitLine("{0} {1} {{ get; set; }}", p.runtimeType, p.name);
//          }

//          foreach (BoltAssetProperty p in m.triggerProperties) {
//            file.EmitLine("void {0}();", p.name);
//            file.EmitLine("Action on{0} {{ get; set; }}", p.name);
//          }

//          file.EmitLine("float this[int layer] { get; set; }");
//          file.EmitLine("UnityEngine.Animator animator { get; }");
//        });

//        file.EmitScope("class {0} : BoltMecanimFrame", m.frameclassName, () => {
//          foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//            file.EmitLine("public {0} {1};", p.runtimeType, p.name);
//          }

//          foreach (BoltAssetProperty p in m.triggerProperties) {
//            file.EmitLine("public bool {0};", p.name);
//          }

//          file.EmitLine("public float[] layerWeights;");

//          // clone method
//          file.EmitScope("public override BoltMecanimFrame Clone ()", () => {
//            file.EmitLine("{0} obj = new {0}();", m.frameclassName);

//            foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//              file.EmitLine("obj.{0} = this.{0};", p.name);
//            }

//            file.EmitLine("return obj;");
//          });

//          // free method
//          file.EmitScope("public override void Free ()", () => {
//            foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//              file.EmitLine("this.{0} = default({1});", p.name, p.runtimeType);
//            }
//          });
//        });

//        file.EmitScope("class {0} : BoltMecanimAnimator<{2}>, {1}", m.name, m.interfaceName, m.frameclassName, () => {
//          // mecanim ids
//          file.EmitLine("// mecanim ids");
//          foreach (BoltAssetProperty p in m.allProperties) {
//            file.EmitLine("static readonly int {0} = UnityEngine.Animator.StringToHash(\"{1}\");", MecanimIdField(p), p.name);
//          }

//          // new backing fields
//          file.EmitLine("// backing fields");
//          foreach (BoltAssetProperty p in m.allProperties) {
//            file.EmitLine("{0} {1};", MecanimRuntimeType(p), MecanimFieldName(p));
//          }

//          // interface implementation (non-trigger)
//          foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//            file.EmitScope("{0} {2}.{1}", p.runtimeType, p.name, m.interfaceName, () => {
//              file.EmitScope("get", () => {
//                file.EmitLine("return {0};", MecanimValue(p));
//              });
//              file.EmitScope("set", () => {
//                file.EmitScope("if ({0}.NotSame(value))", MecanimFieldName(p), () => {
//                  file.EmitLine("{0}.Set(value);", MecanimFieldName(p));
//                  file.EmitLine("_state.PropertyChanged(this);");
//                });
//              });
//            });
//          }

//          // interface implementation (triggers)
//          foreach (BoltAssetProperty p in m.triggerProperties) {
//            file.EmitScope("Action {0}.on{1}", m.interfaceName, p.name, () => {
//              file.EmitScope("get", () => {
//                file.EmitLine("return {0}.callback;", MecanimFieldName(p));
//              });
//              file.EmitScope("set", () => {
//                file.EmitLine("{0}.callback = value;", MecanimFieldName(p));
//              });
//            });

//            file.EmitScope("void {0}.{1} ()", m.interfaceName, p.name, () => {
//              file.EmitLine("AlignTriggerFrame(BoltCore.frame);");
//              file.EmitLine("{0} |= 1UL;", MecanimValue(p));
//              file.EmitLine("_state.PropertyChanged(this);");
//            });
//          }

//          // interface implementation animator getter
//          file.EmitLine("UnityEngine.Animator {0}.animator {{ get {{ return _animator; }} }}", m.interfaceName);


//          // interface implementation (layer weights)

//          file.EmitScope("float {0}.this[int layer]", m.interfaceName, () => {

//            if (m.replicateLayerWeights) {
//              file.EmitLine("get { return GetLayerWeight(layer); }");
//              file.EmitLine("set { SetLayerWeight(layer, value); }");
//            } else {
//              file.EmitLine("get {{ BoltLog.Warn(\"Mecanim animator {0} is not setup to replicate layer weights\"); return -1f; }}", m.name);
//              file.EmitLine("set {{ BoltLog.Warn(\"Mecanim animator {0} is not setup to replicate layer weights\"); }}", m.name);
//            }
//          });

//          // constructor
//          file.EmitScope("public {0} (IBoltState state, BoltEntity entity) : base(entity, state)", m.name, () => {
//            foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//              file.EmitLine("{0} = {1};", MecanimValue(p), FindDefaultValue(m, p));
//            }
//          });

//          // pack method
//          file.EmitScope("public override void Pack (BoltEntityUpdateInfo info, UdpStream stream)", () => {

//            // parameters
//            foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//              EmitWrite(file, p, MecanimValue(p), "info.connection");
//            }

//            // triggers
//            file.EmitLine("AlignTriggerFrame(BoltCore.frame);");

//            foreach (BoltAssetProperty p in m.triggerProperties) {
//              file.EmitLine("stream.WriteULong({0}, _entity.boltSendRate);", MecanimValue(p));
//            }

//            // layer weights
//            if (m.replicateLayerWeights) {
//              file.EmitScope("for (int i = 0; i < _layerWeights.Length; ++i)", () => {
//                file.EmitLine("stream.WriteHalf(_layerWeights[i]);");
//              });
//            }
//          });

//          // read method
//          file.EmitScope("public override void Read (BoltEntityUpdateInfo info, UdpStream stream)", () => {
//            file.EmitLine("{0} f = new {0}();", m.frameclassName);
//            file.EmitLine("f._frame = info.frame;");
//            file.EmitLine("f._triggerOnly = false;");

//            if (m.replicateLayerWeights) {
//              file.EmitLine("f._layerWeights = new float[_layerWeights.Length];");
//            }

//            // parameters
//            foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//              EmitRead(file, p, "f.{0}", "info.connection");
//            }

//            // triggers
//            foreach (BoltAssetProperty p in m.triggerProperties) {
//              file.EmitLine("ulong {0}_val = stream.ReadULong(_entity.boltSendRate);", p.name);
//            }

//            if (m.triggerProperties.Count() > 0) {
//              file.EmitScope("for (int i = _entity.boltSendRate - 1; i > 0; --i)", () => {
//                file.EmitLine("ulong bit = 1UL << i;");

//                foreach (BoltAssetProperty p in m.triggerProperties) {
//                  file.EmitLine("bool {0}_check = ({0}_val & bit) == bit;", p.name);
//                }

//                file.EmitScope("if ({0})", m.triggerProperties.Select(x => x.name + "_check").Join(" || "), () => {
//                  file.EmitLine("{0} tf = new {0}();", m.frameclassName);
//                  file.EmitLine("tf._frame = info.frame - i;");
//                  file.EmitLine("tf._triggerOnly = true;");

//                  foreach (BoltAssetProperty p in m.triggerProperties) {
//                    file.EmitLine("tf.{0} = {0}_check;", p.name);
//                  }

//                  file.EmitLine("_buffer.AddLast(tf);");
//                });
//              });
//            }

//            foreach (BoltAssetProperty p in m.triggerProperties) {
//              file.EmitLine("f.{0} = ({0}_val & 1UL) == 1UL;", p.name);
//            }

//            if (m.replicateLayerWeights) {
//              // layer weights
//              file.EmitScope("for (int i = 0; i < _layerWeights.Length; ++i)", () => {
//                file.EmitLine("f._layerWeights[i] = stream.ReadHalf();");
//              });
//            }

//            // store on buffer
//            file.EmitLine("_buffer.AddLast(f);");
//          });

//          // skip method
//          file.EmitScope("public override void Skip (BoltEntityUpdateInfo info)", () => {

//          });

//          // beforestep method
//          file.EmitScope("public override void BeforeStep ()", () => {
//            file.EmitLine("if (_entity.boltIsOwner || _entity.boltIsControlling) { return; }");

//            file.EmitScope("while (_buffer.count > 0 && _buffer.first._frame <= _entity.boltFrame)", () => {
//              file.EmitLine("var f = _buffer.RemoveFirst();");
//              file.EmitLine("var iface = ({0})this;", m.interfaceName);

//              file.EmitScope("if (f._triggerOnly == false)", () => {
//                // non triggers
//                foreach (BoltAssetProperty p in m.nonTriggerProperties) {
//                  file.EmitLine("iface.{0} = f.{0};", p.name);
//                }

//                if (m.replicateLayerWeights) {
//                  // layer weights
//                  file.EmitScope("for (int i = 0; i < _layerWeights.Length; ++i)", () => {
//                    file.EmitLine("iface[i] = f._layerWeights[i];");
//                  });
//                }
//              });

//              foreach (BoltAssetProperty p in m.triggerProperties) {
//                file.EmitScope("if (f.{0})", p.name, () => {
//                  file.EmitLine("iface.{1}();", m.interfaceName, p.name);
//                });
//              }

//              file.EmitLine("f.Dispose();");
//            });
//          });

//          // afterstep method
//          file.EmitScope("public override void AfterStep ()", () => {
//            file.EmitLine("AlignTriggerFrame(BoltCore.frame);");

//            file.EmitLine("if (_animator.enabled == false) return;");

//            // set values
//            foreach (BoltAssetProperty p in m.allProperties) {
//              if (p.type == BoltAssetPropertyType.Float) {
//                file.EmitLine("_animator.SetFloat({0}, {1}, {2}f, BoltCore.frameDeltaTime);", MecanimIdField(p), MecanimValue(p), p.assetSettingsMecanim.interpolationTime.ToString(CultureInfo.InvariantCulture));

//              } else if (p.type == BoltAssetPropertyType.Int) {
//                file.EmitLine("_animator.SetInteger({0}, {1});", MecanimIdField(p), MecanimValue(p));

//              } else if (p.type == BoltAssetPropertyType.Bool) {
//                file.EmitLine("_animator.SetBool({0}, {1});", MecanimIdField(p), MecanimValue(p));

//              } else if (p.type == BoltAssetPropertyType.Trigger) {
//                file.EmitScope("if (({0} & 1UL) == 1UL)", MecanimValue(p), () => {
//                  file.EmitLine("_animator.SetTrigger({0});", MecanimIdField(p));
//                  file.EmitLine("InvokeAction({0});", MecanimCallback(p));
//                });
//              }
//            }
//          });

//          // aligntriggerframe method
//          file.EmitScope("void AlignTriggerFrame(int frame)", () => {
//            file.EmitScope("if (_triggerFrame < frame)", () => {
//              file.EmitLine("int diff = frame - _triggerFrame;");
//              file.EmitLine("_triggerFrame = frame;");

//              file.EmitScope("if (diff > 63)", () => {
//                foreach (BoltAssetProperty p in m.triggerProperties) {
//                  file.EmitLine("{0} = 0u;", MecanimValue(p));
//                }
//              });

//              file.EmitScope("else", () => {
//                foreach (BoltAssetProperty p in m.triggerProperties) {
//                  file.EmitLine("{0} <<= diff;", MecanimValue(p));
//                }
//              });
//            });
//          });
//        });
//      }
//    }
//  }


//  static string MecanimFieldName (BoltAssetProperty p) {
//    return p.backingFieldName;
//  }

//  static string MecanimRuntimeType (BoltAssetProperty p) {
//    switch (p.type) {
//      case BoltAssetPropertyType.Bool: return "BoltMecanimBoolean";
//      case BoltAssetPropertyType.Float: return "BoltMecanimFloat";
//      case BoltAssetPropertyType.Int: return "BoltMecanimInteger";
//      case BoltAssetPropertyType.Trigger: return "BoltMecanimTrigger";
//    }

//    throw new System.NotSupportedException();
//  }

//  static string MecanimCallback (BoltAssetProperty p) {
//    return MecanimFieldName(p) + ".callback";
//  }

//  static string MecanimValue (BoltAssetProperty p) {
//    return MecanimFieldName(p) + ".value";
//  }

//  static string MecanimIdField (BoltAssetProperty p) {
//    return p.backingFieldName + "mecanimId";
//  }

//  static string FindDefaultValue (BoltMecanimAsset m, BoltAssetProperty p) {
//    AnimatorController ctrl = ((AnimatorController) m.controller);

//    for (int i = 0; i < ctrl.parameterCount; ++i) {
//      AnimatorControllerParameter param = ctrl.GetParameter(i);

//      if (param.name == p.name) {
//        switch (param.type) {
//          case AnimatorControllerParameterType.Float:
//            return param.defaultFloat.ToString(CultureInfo.InvariantCulture) + "f";

//          case AnimatorControllerParameterType.Int:
//            return param.defaultInt.ToString(CultureInfo.InvariantCulture);

//          case AnimatorControllerParameterType.Bool:
//            return param.defaultBool.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
//        }
//      }
//    }

//    throw new KeyNotFoundException();
//  }
//}
