using System;
using UdpKit;
using UnityEngine;

public static class BoltNetworkUtils {
  public static Action Combine (this Action self, Action action) {
    return (Action) Delegate.Combine(self, action);
  }

  public static UdpPlatform CreateUdpPlatform () {
#if UNITY_ANDROID || UNITY_IPHONE
    return new UdpPlatformMobile();
#else
    return new UdpPlatformManaged();
#endif
  }

  public static UdpIPv4Address FindBroadcastAddress () {
    try {
#if UNITY_ANDROID && !UNITY_EDITOR
      Android.AcquireMulticastLock();
      return Android.GetBroadcastAddress();
#elif UNITY_IPHONE && !UNITY_EDITOR
      return UdpPlatformMobile.FindBroadcastAddress_IOS();
#else
      return UdpPlatformManaged.FindBroadcastAddress();
#endif
    } catch (Exception exn) {
      BoltLog.Warning("FindBroadcastAddress threw exception: '{0}' {1}, using 255.255.255.255", exn.GetType().FullName, exn.Message);
      return new UdpIPv4Address(255, 255, 255, 255);
    }
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
