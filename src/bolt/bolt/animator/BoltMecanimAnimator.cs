﻿using System;
using UdpKit;
using UnityEngine;

internal abstract class BoltMecanimAnimator<T> : IBoltStateProperty where T : BoltMecanimFrame, new() {
  protected int _triggerFrame;

  protected IBoltState _state;
  protected BoltEntity _entity;
  protected Animator _animator;
  protected BoltSingleList<T> _buffer;

  public int mecanimFrame {
    get { return (_entity.boltIsOwner || _entity.boltIsControlling) ? BoltCore.frame : _entity.boltFrame; }
  }

  public BoltMecanimAnimator (BoltEntity entity, IBoltState state)
    : this(entity, state, entity.GetComponentInChildren<Animator>()) {
  }

  public BoltMecanimAnimator (BoltEntity entity, IBoltState state, Animator animator) {
    _state = state;
    _entity = entity;
    _animator = animator;
    _buffer = new BoltSingleList<T>();
  }

  public abstract void Pack (BoltEntityUpdateInfo info, UdpStream stream);
  public abstract void Read (BoltEntityUpdateInfo info, UdpStream stream);
  public abstract void Skip (BoltEntityUpdateInfo info);

  public abstract void BeforeStep ();
  public abstract void AfterStep ();

  public void UpdateRender () { }
  public void Teleported () { }
  public void OriginChanging (Transform old, Transform @new) { }

  protected static void InvokeAction (Action action) {
    if (action != null) {
      try {
        action();
      } catch (Exception exn) {
        BoltLog.Exception(exn);
      }
    }
  }

}
