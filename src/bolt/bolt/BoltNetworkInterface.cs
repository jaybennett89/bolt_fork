using System;
using UdpKit;

internal interface IBoltNetwork {
  bool isUnityPro { get; }
  void Setup ();
  void Reset ();
}