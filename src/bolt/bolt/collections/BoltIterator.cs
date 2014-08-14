public struct BoltIterator<T> where T : class, IBoltListNode {
  T _node;
  int _count;
  int _number;

  public T val;

  public BoltIterator (T node, int count) {
    _node = node;
    _count = count;
    _number = 0;
    val = default(T);
  }

  public bool Next () {
    return Next(out val);
  }

  public bool Next (out T item) {
    if (_number < _count) {
      item = _node;

      _node = (T) _node.next;
      _number += 1;

      return true;
    }

    item = default(T);
    return false;
  }
}