using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[DocumentationAttribute]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class BoltGlobalBehaviourAttribute : Attribute {
  public BoltNetworkModes Mode {
    get;
    private set;
  }

  public string[] Scenes {
    get;
    private set;
  }

  public BoltGlobalBehaviourAttribute()
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client) {
  }

  public BoltGlobalBehaviourAttribute(BoltNetworkModes mode)
    : this(mode, new string[0]) {
  }

  public BoltGlobalBehaviourAttribute(params string[] scenes)
    : this(BoltNetworkModes.Server | BoltNetworkModes.Client, scenes) {
  }

  public BoltGlobalBehaviourAttribute(BoltNetworkModes mode, params string[] scenes) {
    this.Mode = mode;
    this.Scenes = scenes;
  }
}