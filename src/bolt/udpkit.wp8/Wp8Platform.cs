using System.Collections.Generic;

#if WINDOWS_PHONE
using Windows.Networking.Connectivity;
#endif

namespace UdpKit {
  public static class Wp8Platform {
    public static string[] GetIpAddresses() {
#if WINDOWS_PHONE
      var addresses = new List<string>();

      foreach (var host in NetworkInformation.GetHostNames()) {
        if (host.IPInformation == null) {
          continue;
        }

        addresses.Add(host.DisplayName);
      }

      return addresses.ToArray();
#else
      return new string[0];
#endif
    }
  }
}