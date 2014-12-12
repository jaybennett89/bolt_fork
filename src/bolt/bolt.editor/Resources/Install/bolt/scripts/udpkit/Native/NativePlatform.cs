﻿#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
using UnityEngine;
using System.Collections;

public class NativePlatform : UdpKit.UdpPlatform {
  static readonly object timeLock = new object();

  public override UdpKit.UdpPlatformSocket CreateSocket() {
    return new NativeSocket(this);
  }

  public override UdpKit.UdpIPv4Address GetBroadcastAddress() {
#if UNITY_IPHONE
    return NativePInvoke.GetBroadcastAddress();
#elif UNITY_ANDROID
    return Android.GetBroadcastAddress();
#else
    return new UdpKit.UdpIPv4Address(255, 255, 255, 255);
#endif
  }

  public override System.Collections.Generic.List<UdpKit.UdpPlatformInterface> GetNetworkInterfaces() {
    throw new System.NotImplementedException();
  }

  public override uint GetPrecisionTime() {
    lock (timeLock) {
      return NativePInvoke.GetHighPrecisionTime();
    }
  }

  public override bool SupportsBroadcast {
    get { return true; }
  }

#if UNITY_ANDROID
  static class Android {
    static AndroidJavaObject androidMulticastLock;

    public static UdpIPv4Address GetBroadcastAddress () {
      AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
      AndroidJavaObject wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
      AndroidJavaObject dhcp = wifi.Call<AndroidJavaObject>("getDhcpInfo");

      int dhcp_ip = dhcp.Get<int>("ipAddress");
      int dhcp_mask = dhcp.Get<int>("netmask");
      int broadcast = (dhcp_ip & dhcp_mask) | ~dhcp_mask;
      byte[] quads = new byte[4];

      for (int k = 0; k < 4; k++) {
        quads[k] = (byte) ((broadcast >> k * 8) & 0xFF);
      }

      return new UdpIPv4Address(quads[0], quads[1], quads[2], quads[3]);
    }

    public static void AcquireMulticastLock () {
      AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
      AndroidJavaObject wifi = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
      androidMulticastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "udpkit");
      androidMulticastLock.Call("acquire");
    }

    public static void ReleaseMulticastLock () {
      if (androidMulticastLock != null) {
        androidMulticastLock.Call("release");
        androidMulticastLock.Dispose();
        androidMulticastLock = null;
      }
    }
  }
#endif
}
#endif