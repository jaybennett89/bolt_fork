using System;
using System.Collections.Generic;
using System.Text;

namespace UdpKit.Protocol {
  abstract class Msg {
    public const byte MESSAGE_HEADER = 254;
    public const byte MESSAGE_HEADER_MULTI = 255;

    int _ptr;
    byte[] _buffer;

    byte _type;
    bool _pack;

    public Guid PeerId;
    public Guid MessageId;
    public UdpEndPoint Sender;
    public UdpEndPoint Target;

    public uint SendTime;
    public uint Timeout;
    public uint Attempts;

    public bool Read {
      get { return !Pack; }
    }

    public bool Pack {
      get { return _pack; }
    }

    public Msg() {
      MessageId = Guid.NewGuid();
    }

    public void Init(byte type) {
      _type = type;
    }

    public void InitBuffer(int ptr, byte[] buffer, bool pack) {
      _ptr = ptr;
      _pack = pack;
      _buffer = buffer;
    }

    public int Serialize(int ptr, byte[] buffer, bool pack) {
      InitBuffer(ptr, buffer, pack);
      return Serialize();
    }

    public int Serialize() {
      Serialize(ref _type);
      Serialize(ref PeerId);
      Serialize(ref MessageId);

      OnSerialize();

      // clear data buffer
      _buffer = null;

      return _ptr;
    }

    public override string ToString() {
      return GetType().Name;
    }

    protected void Create<T>(ref T value) where T : class, new() {
      if (_pack) {
        UdpAssert.Assert(value != null);
      }
      else {
        value = new T();
      }
    }

    protected void Serialize<T>(ref T value) where T : struct {
      UdpAssert.Assert(typeof(T).IsEnum);

      if (_pack) {
        Blit.PackI32(_buffer, ref _ptr, EnumToInt<T>(value));
      }
      else {
        value = IntToEnum<T>(Blit.ReadI32(_buffer, ref _ptr));
      }
    }

    protected void Serialize(ref UdpSession session) {
      Create(ref session);

      Serialize(ref session._id);
      Serialize(ref session._hostName);
      Serialize(ref session._hostData);
      Serialize(ref session._lanEndPoint);
      Serialize(ref session._wanEndPoint);
    }

    protected void Serialize(ref byte value) {
      if (_pack) {
        if (HasSpace(1)) {
          Blit.PackByte(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadByte(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref int value) {
      if (_pack) {
        if (HasSpace(4)) {
          Blit.PackI32(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadI32(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref uint value) {
      if (_pack) {
        if (HasSpace(4)) {
          Blit.PackU32(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadU32(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref string value) {
      if (_pack) {
        if (HasSpace(Blit.GetStringSize(value))) {
          Blit.PackString(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadString(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref byte[] value) {
      if (_pack) {
        if (HasSpace(Blit.GetBytesPrefixSize(value))) {
          Blit.PackBytesPrefix(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadBytesPrefix(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref Guid value) {
      if (_pack) {
        if (HasSpace(16)) {
          Blit.PackGuid(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadGuid(_buffer, ref _ptr);
      }
    }

    protected void Serialize(ref UdpEndPoint value) {
      if (_pack) {
        if (HasSpace(6)) {
          Blit.PackEndPoint(_buffer, ref _ptr, value);
        }
      }
      else {
        value = Blit.ReadEndPoint(_buffer, ref _ptr);
      }
    }

    protected virtual void OnSerialize() {

    }

    bool HasSpace(int bytes) {
      if ((_ptr >= 0) && ((_ptr + bytes) <= _buffer.Length)) {
        return true;
      }

      _ptr = -1;
      return false;
    }

    static int EnumToInt<T>(T value) where T : struct {
      return (int)(ValueType)value;
    }

    static T IntToEnum<T>(int value) where T : struct {
      return (T)(ValueType)value;
    }
  }
}
