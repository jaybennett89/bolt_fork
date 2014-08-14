using UdpKit;

class BoltSerializer : UdpSerializer<BoltPacket> {
  public override bool Pack (UdpStream stream, BoltPacket input, out BoltPacket sent) {
    Assert.False(input.pooled);

    int writeOffset = 0;
    int writeLength = BoltMath.BytesRequired(input.stream.Position);

    // we always send the entire thing
    sent = input;

    // copy data from input stream to network stream
    stream.WriteByteArray(input.stream.ByteBuffer, writeOffset, writeLength);

    // done!
    return true;
  }

  public override bool Unpack (UdpStream stream, out BoltPacket received) {
    int readOffset = BoltMath.BytesRequired(stream.Position);
    int readLength = BoltMath.BytesRequired(stream.Size - stream.Position);

    // allocate a new packet and stream and copy data
    received = BoltPacketPool.Acquire();
    received.stream.WriteByteArray(stream.ByteBuffer, readOffset, readLength);
    received.stream.Position = 0;
    received.stream.Size = stream.Size - stream.Position;
    Assert.False(received.pooled);

    // done!
    return true;
  }
}
