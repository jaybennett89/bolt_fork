using UdpKit;
using UnityEngine;

public abstract class BoltStateTransformDeadReckoning : BoltStateTransform, IBoltStateProperty {

  protected int _frameCount;
  protected int _interpCount;

  protected Frame _fr;
  protected Frame _to;

  protected struct Frame {
    public int frame;
    public float acceleration;
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
  }

  protected bool _teleport;
  protected Frame _owner;
  protected BoltRingBuffer<Frame> _buffer;
  protected int _maxForwardExtrap = 2;
  protected int _maxInterpTime = 1;

  protected bool _useAcceleration = false;
  protected bool _inferVelocity = false;
  protected bool _inferAcceleration = false;
  protected float _toleranceVelocity = 0.01f;
  protected float _toleranceAcceleration = 0.01f;

  protected bool _zeroVelocityX = false;
  protected bool _zeroVelocityY = false;
  protected bool _zeroVelocityZ = false;

  protected BoltStateTransformDeadReckoning (IBoltState state, BoltEntity entity)
    : base(entity, state) {
    _state = state;
    _entity = entity;
    _buffer = new BoltRingBuffer<Frame>(20);
    _buffer.autofree = true;

    _fr.rotation = _to.rotation = transformRotation;
    _fr.position = _to.position = transformPosition;
    _fr.velocity = _to.velocity = Vector3.zero;
    _fr.acceleration = _to.acceleration = 0f;

    _owner = new Frame();
    _owner.velocity = Vector3.zero;
    _owner.position = transformPosition;
    _owner.rotation = transformRotation;
    _owner.acceleration = 0f;
  }

  public virtual void Pack (BoltEntityUpdateInfo info, UdpStream stream) {
    PackFrame(info, stream);
  }

  public virtual void Read (BoltEntityUpdateInfo info, UdpStream stream) {
    ReadFrame(info, stream);

    if (info.first || _teleport) {
      transformPosition = _to.position = _fr.position = _buffer.last.position;
      transformRotation = _to.rotation = _fr.rotation = _buffer.last.rotation;

      _teleport = false;
    }
  }

  public abstract void PackFrame (BoltEntityUpdateInfo info, UdpStream stream);
  public abstract void ReadFrame (BoltEntityUpdateInfo info, UdpStream stream);

  public virtual void Skip (BoltEntityUpdateInfo info) {
    Vector3 pos;
    Quaternion rot;

    if (_buffer.count > 0) {
      pos = _buffer.last.position;
      rot = _buffer.last.rotation;
    } else {
      pos = _to.position;
      rot = _to.rotation;
    }

    _buffer.Enqueue(new Frame {
      frame = info.frame,
      position = pos,
      rotation = rot,
      velocity = Vector3.zero,
      acceleration = 0f
    });
  }

  public virtual void BeforeStep () {
    if (shouldStep == false) {
      return;
    }

    int rate = _entity.boltSendRate;
    var ratef = (float) rate;

    int interp = rate * (1 + _maxInterpTime);
    var interpf = (float) interp;

    int frames = rate * (1 + _maxForwardExtrap);
    var framesf = (float) frames;

    _frameCount = Mathf.Clamp(_frameCount + 1, 0, frames);
    _interpCount = Mathf.Clamp(_interpCount + 1, 0, interp);

  RESTART:

    Vector3 posFrom = Extrapolate(_fr.position, _fr.velocity, _fr.acceleration, _frameCount);
    Vector3 posTo = Extrapolate(_to.position, _to.velocity, _to.acceleration, _frameCount);

    transformPosition = Vector3.Lerp(posFrom, posTo, _interpCount / interpf);
    transformRotation = Quaternion.Lerp(_fr.rotation, _to.rotation, _interpCount / interpf);

    while (_buffer.count > 0) {
      if (_buffer.first.frame <= _entity.boltFrame) {
        var f = _buffer.Dequeue();

        if (f.frame == _entity.boltFrame) {

          if (_inferVelocity) {
            f.velocity = (f.position - _to.position) / ratef;
            if (_zeroVelocityX) { f.velocity.x = 0; }
            if (_zeroVelocityY) { f.velocity.y = 0; }
            if (_zeroVelocityZ) { f.velocity.z = 0; }
          }

          _fr.position = transformPosition;
          _to.position = f.position;

          _fr.velocity = f.velocity;
          _to.velocity = f.velocity;

          _fr.acceleration = f.acceleration;
          _to.acceleration = f.acceleration;

          _fr.rotation = transformRotation;
          _to.rotation = f.rotation;

          _frameCount = 0;
          _interpCount = 0;

        } else {
          if (_inferVelocity) {
            f.velocity = (f.position - _to.position) / ratef;
            if (_zeroVelocityX) { f.velocity.x = 0; }
            if (_zeroVelocityY) { f.velocity.y = 0; }
            if (_zeroVelocityZ) { f.velocity.z = 0; }
          }

          // TODO: Make this handle 2+ skipped frames better
          int exterp = _frameCount - (_frameCount % rate);

          _fr.position = Extrapolate(_to.position, _to.velocity, _to.acceleration, exterp);
          _to.position = f.position;

          _fr.acceleration = _to.acceleration;
          _to.acceleration = f.acceleration;

          _fr.velocity = _to.velocity;
          _to.velocity = f.velocity;

          _fr.rotation = transformRotation;
          _to.rotation = f.rotation;

          _frameCount = _frameCount % rate;
          _interpCount = 0;
        }

        if (_to.velocity.magnitude < _toleranceVelocity && (_useAcceleration == false || (_to.acceleration < _toleranceAcceleration))) {
          _to.position = _fr.position = transformPosition;
          _to.velocity = _fr.velocity = Vector3.zero;
          _to.acceleration = _fr.acceleration = 0f;
        }

        goto RESTART;

      } else {
        break;
      }
    }

  }

  public virtual void AfterStep () {
    Vector3 p = transformPosition;
    Vector3 v = p - _owner.position;
    Quaternion r = transformRotation;
    float a = v.magnitude - _owner.velocity.magnitude;

    if (p != _owner.position || r != _owner.rotation) {
      _state.PropertyChanged(this);
    }

    _owner.position = p;
    _owner.velocity = v;
    _owner.rotation = r;
    _owner.acceleration = a;
  }

  public virtual void OriginChanging (Transform old, Transform @new) {

  }

  public virtual void UpdateRender () {

  }

  protected Vector3 Extrapolate (Vector3 p, Vector3 v, float a, int t) {
    if (_useAcceleration) {
      return p + (v * t) + (0.5f * a * t * t * v);
    } else {
      return p + (v * t);
    }
  }

  public void Teleported () {
    _buffer.Clear();
    _teleport = true;
  }
}
