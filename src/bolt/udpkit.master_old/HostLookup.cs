using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit {
  class HostLookup {
    readonly UdpPlatform platform;
    readonly List<UdpSession> hosts = new List<UdpSession>();

    public IEnumerable<UdpSession> All {
      get { return hosts; }
    }

    public HostLookup(UdpPlatform p) {
      platform = p;
    }

    public void KeepAlive(Guid id) {
      UdpSession host;

      if (Find(id, out host)) {
        host._lastSeen = platform.GetPrecisionTime();
      }
    }

    public bool Find(Guid id, out UdpSession host) {
      host = hosts.FirstOrDefault(x => x.Id == id);
      return host != null;
    }

    public void Update(UdpSession host) {
      int index = hosts.FindIndex(x => x.Id == host.Id);

      if (index != -1) {
        hosts[index] = host;
      }
      else {
        hosts.Add(host);
      }
    }

    public void Remove(UdpSession host) {
      hosts.Remove(host);
    }

    public void Timeout(uint now) {
      for (int i = 0; i < hosts.Count; ++i) {
        if (hosts[i]._lastSeen + 60000 < now) {
          // remove from list
          hosts.RemoveAt(i);

          // step back counter
          --i;
        }
      }
    }
  }
}
