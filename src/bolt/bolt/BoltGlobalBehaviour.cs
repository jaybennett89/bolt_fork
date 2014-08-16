using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class BoltGlobalBehaviourAttribute : Attribute {
  public BoltNetworkModes mode {
    get;
    private set;
  }

  public string[] maps {
    get;
    private set;
  }

  public BoltGlobalBehaviourAttribute ()
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client) {
  }

  public BoltGlobalBehaviourAttribute (BoltNetworkModes mode)
    : this(mode, new string[0]) {
  }

  public BoltGlobalBehaviourAttribute (params string[] maps)
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client, maps) {
  }

  public BoltGlobalBehaviourAttribute (BoltNetworkModes mode, params string[] maps) {
    this.mode = mode;
    this.maps = maps;
  }
}