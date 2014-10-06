internal interface IBoltSequenced {
  uint Sequence { get; set; }
}

internal enum RecvBufferAddResult {
  Old,
  OutOfBounds,
  AlreadyExists,
  Added
}

internal class BoltRecvBuffer<T> where T : IBoltSequenced {
  struct Node {
    public bool Received;
    public T Value;
  }

  int tail;
  int mask;
  int sequenceShift;
  uint sequenceNext;
  uint sequenceMask;
  readonly Node[] nodes;

  public BoltRecvBuffer (int windowBits, int sequenceBits) {
    nodes = new Node[1 << windowBits];
    mask = nodes.Length - 1;

    sequenceShift = 32 - sequenceBits;
    sequenceMask = (1u << sequenceBits) - 1u;
    sequenceNext = 0;
  }

  public bool TryRemove (out T value) {
    Node n = nodes[tail];

    if (n.Received) {
      value = n.Value;
      nodes[tail] = default(Node);

      tail += 1;
      tail &= mask;

      sequenceNext = value.Sequence + 1u;
      sequenceNext &= sequenceMask;
    } else {
      value = default(T);
    }

    return n.Received;
  }

  public RecvBufferAddResult TryEnqueue (T value) {
    int distance = BoltMath.SequenceDistance(value.Sequence, sequenceNext, sequenceShift);
    int index = (tail + distance) & mask;

    if (distance <= -nodes.Length || distance >= nodes.Length) {
      return RecvBufferAddResult.OutOfBounds;
    }

    if (distance < 0) {
      return RecvBufferAddResult.Old;
    }

    if (nodes[index].Received) {
      return RecvBufferAddResult.AlreadyExists;
    }

    nodes[index].Received = true;
    nodes[index].Value = value;
    return RecvBufferAddResult.Added;
  }

}
