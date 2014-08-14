using System;
using UdpKit;

internal interface IBoltNetwork {
  Type loadBehaviourType { get; }

  void Setup ();
  void Reset ();
}