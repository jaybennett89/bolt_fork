//using UdpKit;
//using UnityEngine;
//using System.Linq;

//public abstract class BoltStateTransformInterpolatedSnapshots : BoltStateTransform, IBoltStateProperty {
//  protected struct Frame {
//    public int frame;
//    public bool extrapolted;

//    public Vector3 pos;
//    public Quaternion rot;

//    public Frame (int f, Vector3 p, Quaternion r) {
//      pos = p;
//      rot = r;
//      frame = f;
//      extrapolted = false;
//    }
//  }

//  protected bool _teleport;
//  protected bool _posChanged;
//  protected bool _rotChanged;

//  protected Frame _current;
//  protected BoltRingBuffer<Frame> _buffer;

//  protected BoltStateTransformInterpolatedSnapshots (IBoltState state, BoltEntity entity)
//    : base(entity, state) {
//    _state = state;
//    _transform = entity.transform;

//    _buffer = new BoltRingBuffer<Frame>(20);
//    _buffer.autofree = true;
//  }

//  //bool CompareFloats (float a, float b, float accuracy) {
//  //  return Mathf.Abs(a - b) < accuracy;
//  //}

//  public void AfterStep () {
//    //if (_entity.boltIsOwner && _entity.boltIsControlling)
//    //  BoltLog.Info("{3} TRANSFORM ROT = X:{0} Y:{1} Z:{2}, CURRENT ROT = X:{4} Y:{5} Z:{6}", transformRotation.eulerAngles.x, transformRotation.eulerAngles.y, transformRotation.eulerAngles.z, _entity.gameObject.name, _current.rot.eulerAngles.x, _current.rot.eulerAngles.y, _current.rot.eulerAngles.z);

//    _posChanged = _current.pos != transformPosition;
//    _rotChanged = _current.rot.eulerAngles != transformRotation.eulerAngles;

//    //CompareFloats(currentRot.x, transformRot.x, 1f) == false ||
//    //CompareFloats(currentRot.y, transformRot.y, 1f) == false ||
//    //CompareFloats(currentRot.z, transformRot.z, 1f) == false;

//    if (_posChanged || _rotChanged) {
//      _state.PropertyChanged(this);
//    }

//    _current.pos = transformPosition;
//    _current.rot = transformRotation;
//  }

//  public void UpdateRender () {
//  }

//  public void OriginChanging (Transform old, Transform @new) {
//    if (_entity.isOwner) {
//      AfterStep();

//    } else {
//      if (old) {
//        if (@new) {
//          UpdateBuffer(old.localToWorldMatrix, @new.worldToLocalMatrix);
//        } else {
//          UpdateBuffer(old.localToWorldMatrix, Matrix4x4.identity);
//        }
//      } else {
//        if (@new) {
//          UpdateBuffer(Matrix4x4.identity, @new.worldToLocalMatrix);
//        }
//      }
//    }
//  }

//  public virtual void BeforeStep () {
//    if (shouldStep == false) {
//      return;
//    }

//    if (_buffer.count == 0) {
//      return;
//    }

//    //// perform extrapolation if needed and possible
//    //if (_buffer.count >= 2 && _buffer.last.frame < _entity.boltFrame && ExtrapoltedCount() < 2) {
//    //  Frame last = _buffer[_buffer.count - 1];
//    //  Frame prev = _buffer[_buffer.count - 2];
//    //  Frame exterp = new Frame();

//    //  int frame = last.frame;

//    //  while (frame < _entity.boltFrame) {
//    //    frame += _entity.boltSendRate;
//    //  }

//    //  exterp.pos = last.pos + (last.pos - prev.pos);
//    //  exterp.rot = Extrapolate(prev.rot, last.rot, 2f);
//    //  exterp.extrapolted = true;
//    //  exterp.frame = frame;

//    //  _buffer.Enqueue(exterp);

//    //  //BoltLog.Debug("extrapolted frame {0} (count: {1}, prev: {2}, last: {3}, remote_diff: {4})", exterp.frame, exterpCount, prev.frame, last.frame, _entity._source.remoteFrameDiff);
//    //}

//    //string bstring = string.Join(", ", _buffer.Select(x => x.frame.ToString()).ToArray());
//    //BoltLog.Info("LOOKUP - _entity.boltFrame: {0}, _buffer: {1}", _entity.boltFrame, bstring);

//    //if (_buffer.count > 0 && _entity.boltFrame > _buffer.last.frame) {
//    //  BoltLog.Info("EXTRAPOLATE GOD-DAMNIT: {0}", _entity.boltSource.remoteFrameDiff);
//    //}

//    UpdatePosition();
//  }

//  public virtual void Pack (BoltEntityUpdateInfo info, UdpStream stream) {
//    bool pos = true; // _posChanged || info.first;
//    bool rot = true; // _rotChanged || info.first;

//    stream.WriteBool(pos);
//    stream.WriteBool(rot);

//    if (stream.WriteBool(_entity._origin)) {
//      stream.WriteInt(BoltCore.resolveTransformId(_entity._origin));
//    }

//    PackFrame(_current, pos, rot, stream);

//    _posChanged = false;
//    _rotChanged = false;
//  }

//  public virtual void Teleported () {
//    _buffer.Clear();
//    _teleport = true;
//  }

//  public virtual void Read (BoltEntityUpdateInfo info, UdpStream stream) {
//    bool pos = stream.ReadBool();
//    bool rot = stream.ReadBool();
//    bool hasOrigin = stream.ReadBool();

//    //if (hasOrigin) {
//    //  _entity.SetOriginInternal(BoltCore.resolveTransform(stream.ReadInt()));
//    //} else {
//    //  _entity.SetOriginInternal(null);
//    //}

//    Frame f = ReadFrame(pos, rot, stream);
//    f.frame = info.frame;

//    if (info.first || _teleport) {
//      transformPosition = f.pos;
//      transformRotation = f.rot;
//      _teleport = false;
//    }

//    if (_buffer.count > 0) {
//      if (_buffer.last.frame >= f.frame) {
//        return;
//      }

//      if (!pos) { f.pos = _buffer.last.pos; }
//      if (!rot) { f.rot = _buffer.last.rot; }
//    }

//    _buffer.Enqueue(f);
//  }

//  protected virtual void PackFrame (Frame frame, bool pos, bool rot, UdpStream stream) {
//    stream.WriteVector3(frame.pos);
//    stream.WriteQuaternion(frame.rot);
//  }

//  protected virtual Frame ReadFrame (bool pos, bool rot, UdpStream stream) {
//    Frame frame = new Frame();
//    frame.pos = stream.ReadVector3();
//    frame.rot = stream.ReadQuaternion();
//    return frame;
//  }

//  public virtual void Skip (BoltEntityUpdateInfo info) {

//  }

//  public virtual void Update () {

//  }

//  protected virtual int ExtrapoltedCount () {
//    int count = 0;

//    for (int i = (_buffer.count - 1); i >= 0; --i) {
//      if (_buffer[i].extrapolted) {
//        count += 1;
//      } else {
//        return count;
//      }
//    }

//    return count;
//  }

//  void UpdateBuffer (Matrix4x4 l2w, Matrix4x4 w2l) {
//    for (int i = 0; i < _buffer.count; ++i) {
//      Frame f = _buffer[i];

//      Vector3 before = f.pos;

//      f.pos = l2w.MultiplyPoint(f.pos);
//      f.pos = w2l.MultiplyPoint(f.pos);

//      Vector3 axis;
//      float angle;

//      f.rot.ToAngleAxis(out angle, out axis);

//      axis = l2w.MultiplyVector(axis);
//      axis = w2l.MultiplyVector(axis);

//      f.rot = Quaternion.AngleAxis(angle, axis);

//      _buffer[i] = f;
//    }
//  }

//  void UpdatePosition () {

//    for (int i = 0; i < _buffer.count; ++i) {
//      Frame t = _buffer[i];

//      if (t.frame < _entity.boltFrame && i == (_buffer.count - 1)) {
//        transformPosition = t.pos;
//        transformRotation = t.rot;
//        return;
//      }

//      if (t.frame >= _entity.boltFrame) {
//        Frame p = _buffer[Mathf.Max(0, i - 1)];

//        //var bstring = string.Join(", ", _buffer.Select(x => x.pos.ToString()).ToArray());
//        //BoltLog.Info("TFRAME - _entity.boltFrame: {0}, t.frame: {1}, _buffer: {2}", _entity.boltFrame, t.frame, bstring);

//        if (p.frame == t.frame) {
//          transformPosition = t.pos;
//          transformRotation = t.rot;

//        } else {
//          //BoltLog.Info("t.pos: {0}, p.pos: {1}, buffer: {2}", t.pos, p.pos, bstring);

//          int pframe = p.frame;

//          if (pframe < t.frame - (_entity.boltSendRate * 2)) {
//            pframe = t.frame - _entity.boltSendRate;
//          }

//          float d = t.frame - pframe;
//          float a = _entity.boltFrame - pframe;

//          transformPosition = Vector3.Lerp(p.pos, t.pos, a / d);
//          transformRotation = Quaternion.Lerp(p.rot, t.rot, a / d);

//          ////BoltLog.Info("frame: {5}, p.frame={0}, t.frame={1}, p.rot={2}, t.rot={3}, a/d={4}", pframe, t.frame, p.rotation, t.rotation, a / d, frame);
//        }

//        return;
//      }
//    }
//  }

//  static Quaternion Extrapolate (Quaternion a, Quaternion b, float factor) {
//    Quaternion diff = b * Quaternion.Inverse(a);

//    float angle;
//    Vector3 axis;

//    diff.ToAngleAxis(out angle, out axis);

//    if (angle > 180) {
//      angle -= 180;
//    }

//    return Quaternion.AngleAxis((angle * factor) % 360, axis) * a;
//  }
//}

