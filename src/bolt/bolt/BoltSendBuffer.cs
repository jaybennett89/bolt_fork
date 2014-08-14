using System.Collections.Generic;

internal class BoltSendBuffer<T> where T : IBoltSequenced {
  enum State {
    Free = 0,
    Send = 1,
    Transit = 2,
    Delivered = 3
  }

  struct Node {
    public State State;
    public T Value;
  }

  int tail;
  int mask;
  int shift;
  int count;

  Node[] nodes;
  BoltSequenceGenerator generator;

  public IEnumerable<T> Pending {
    get {
      T val;

      while (TryNext(out val)) {
        yield return val;
      }
    }
  }

  public IEnumerable<T> Delivered {
    get {
      T val;

      while (TryRemove(out val)) {
        yield return val;
      }
    }
  }

  public bool Full {
    get { return count == nodes.Length; }
  }

  public BoltSendBuffer (int windowBits, int sequenceBits) {
    nodes = new Node[1 << windowBits];
    shift = 32 - sequenceBits;
    mask = nodes.Length - 1;
    generator = new BoltSequenceGenerator(sequenceBits, uint.MaxValue);
  }

  public bool TryEnqueue (T value) {
    int index = -1;

    if (count == 0) {
      index = tail;
    } else {
      if (count == nodes.Length) {
        return false;
      }

      index = (tail + count) & mask;
    }

    nodes[index].Value = value;
    nodes[index].Value.sequence = generator.Next();
    nodes[index].State = State.Send;

    count += 1;
    return true;
  }

  public bool TryNext (out T value) {
    for (int i = 0; i < count; ++i) {
      int index = (tail + i) & mask;

      if (nodes[index].State == State.Send) {
        nodes[index].State = State.Transit;
        value = nodes[index].Value;
        return true;
      }
    }

    value = default(T);
    return false;
  }

  public void SetDelivered (T value) {
    ChangeState(value, State.Delivered);
  }

  public void SetSend (T value) {
    ChangeState(value, State.Send);
  }

  public bool TryRemove (out T value) {
    if (count > 0 && nodes[tail].State == State.Delivered) {
      value = nodes[tail].Value;
      nodes[tail] = default(Node);

      tail += 1;
      tail &= mask;

      count -= 1;
      return true;
    }

    value = default(T);
    return false;
  }

  void ChangeState (T value, State state) {
    if (count == 0) {
      return;
    }

    int distance = SequenceDistance(value.sequence, nodes[tail].Value.sequence);
    if (distance < 0 || distance >= count) {
      return;
    }

    nodes[(tail + distance) & mask].State = state;
  }

  int SequenceDistance (uint from, uint to) {
    from <<= shift;
    to <<= shift;
    return ((int) (from - to)) >> shift;
  }
}
