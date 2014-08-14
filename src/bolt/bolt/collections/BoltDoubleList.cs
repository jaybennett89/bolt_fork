using System;
using System.Collections;
using System.Collections.Generic;

public class BoltDoubleList<T> : IEnumerable<T> where T : class, IBoltListNode {
  T _first;
  int _count;

  public int count {
    get { return _count; }
  }

  public T first {
    get {
      VerifyNotEmpty();
      return _first;
    }
  }

  public T last {
    get {
      VerifyNotEmpty();
      return (T) _first.prev;
    }
  }

  public BoltIterator<T> GetIterator () {
    return new BoltIterator<T>(_first, _count);
  }

  public bool IsFirst (T node) {
    VerifyInList(node);

    if (_count == 0)
      return false;

    return ReferenceEquals(node, _first);
  }

  public void AddLast (T node) {
    VerifyCanInsert(node);

    if (_count == 0) {
      InsertEmpty(node);
    } else {
      InsertBefore(node, _first);
    }
  }

  public void AddFirst (T node) {
    VerifyCanInsert(node);

    if (_count == 0) {
      InsertEmpty(node);
    } else {
      InsertBefore(node, _first);
      _first = node;
    }
  }

  public T Remove (T node) {
    VerifyInList(node);
    VerifyNotEmpty();
    RemoveNode(node);
    return node;
  }

  public T RemoveFirst () {
    return Remove(_first);
  }

  public T RemoveLast () {
    return Remove((T) _first.prev);
  }

  public void Clear () {
    _first = null;
    _count = 0;
  }

  public T Prev (T node) {
    VerifyInList(node);
    return (T) node.prev;
  }

  public T Next (T node) {
    VerifyInList(node);
    return (T) node.next;
  }

  public void Replace (T node, T newNode) {
    VerifyInList(node);
    VerifyCanInsert(newNode);

    // setup new node
    newNode.list = this;
    newNode.next = node.next;
    newNode.prev = node.prev;

    T next = (T) newNode.next;
    T prev = (T) newNode.prev;

    next.prev = newNode;
    prev.next = newNode;

    // if this node is the "first" node, then replace
    if (ReferenceEquals(_first, node)) {
      _first = newNode;
    }

    // clean up old node
    node.list = null;
    node.prev = null;
    node.next = null;
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

  void InsertBefore (T node, T before) {
    node.next = before;
    node.prev = before.prev;

    T prev = (T) before.prev;
    prev.next = (T) node;

    before.prev = node;

    node.list = this;
    ++_count;
  }

  void InsertEmpty (T node) {
    _first = node;
    _first.next = node;
    _first.prev = node;

    node.list = this;
    ++_count;
  }

  void RemoveNode (T node) {
    if (_count == 1) {
      _first = null;
    } else {
      T next = (T) node.next;
      T prev = (T) node.prev;

      next.prev = node.prev;
      prev.next = node.next;

      if (ReferenceEquals(_first, node)) {
        _first = (T) node.next;
      }
    }

    node.list = null;
    --_count;
  }

  void VerifyNotEmpty () {
    if (_count == 0)
      throw new InvalidOperationException("List is empty");
  }

  public IEnumerator<T> GetEnumerator () {
    T n = _first;
    int c = count;

    while (c > 0) {
      yield return n;
      n = (T) n.next;
      c = c - 1;
    }
  }

  IEnumerator IEnumerable.GetEnumerator () {
    return GetEnumerator();
  }

  public static implicit operator bool (BoltDoubleList<T> list) {
    return list != null;
  }
}
