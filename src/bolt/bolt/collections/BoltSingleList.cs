using Bolt;
using System;
using System.Collections;
using System.Collections.Generic;

[Documentation(Ignore = true)]
public class BoltSingleList<T> : IEnumerable<T> where T : class, IBoltListNode {

  T _head;
  T _tail;
  int _count;

  public int count {
    get { return _count; }
  }

  public T first {
    get {
      VerifyNotEmpty();
      return _head;
    }
  }

  public T last {
    get {
      VerifyNotEmpty();
      return _tail;
    }
  }

  public BoltIterator<T> GetIterator () {
    return new BoltIterator<T>(_head, _count);
  }

  public void AddFirst (T item) {
    VerifyCanInsert(item);

    if (_count == 0) {
      _head = _tail = item;
    } else {
      item.next = _head;
      _head = item;
    }

    item.list = this;
    ++_count;
  }

  public void AddLast (T item) {
    VerifyCanInsert(item);

    if (_count == 0) {
      _head = _tail = item;
    } else {
      _tail.next = item;
      _tail = item;
    }

    item.list = this;
    ++_count;
  }

  public T PeekFirst () {
    VerifyNotEmpty();
    return _head;
  }

  public T RemoveFirst () {
    VerifyNotEmpty();

    T result = _head;

    if (_count == 1) {
      _head = _tail = null;
    } else {
      _head = (T) _head.next;
    }

    --_count;
    result.list = null;
    return result;
  }

  public void Clear () {
    _head = null;
    _tail = null;
    _count = 0;
  }

  public T Next (T item) {
    VerifyInList(item);
    return (T) item.next;
  }

  public IEnumerator<T> GetEnumerator () {
    T current = _head;

    while (current != null) {
      yield return current;
      current = (T) current.next;
    }
  }

  void VerifyNotEmpty () {
    if (_count == 0)
      throw new InvalidOperationException("List is empty");
  }

  void VerifyCanInsert (T node) {
    if (ReferenceEquals(node.list, null) == false) {
      throw new InvalidOperationException("Node is already in a list");
    }
  }

  void VerifyInList (T node) {
    if (ReferenceEquals(node.list, this) == false) {
      throw new InvalidOperationException("Node is not in this list");
    }
  }

  IEnumerator IEnumerable.GetEnumerator () {
    return GetEnumerator();
  }

  public static implicit operator bool (BoltSingleList<T> list) {
    return list != null;
  }
}
