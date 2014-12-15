using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt {
  internal class NetworkStorage : BitSet, IBoltListNode {
    public int Frame;
    public NetworkObj Root;
    public NetworkValue[] Values;

    public NetworkStorage(int size) {
      Values = new NetworkValue[size];
    }

    public void PropertyChanged(int property) {
      Set(property);
    }

    object IBoltListNode.prev {
      get;
      set;
    }

    object IBoltListNode.next {
      get;
      set;
    }

    object IBoltListNode.list {
      get;
      set;
    }
  }
}
