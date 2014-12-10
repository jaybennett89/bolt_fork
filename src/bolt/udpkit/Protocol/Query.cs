using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UdpKit.Protocol {
  abstract class Query : Message {
    public Result Result;

    public virtual bool IsUnique {
      get { return false; }
    }

    public virtual bool Resend {
      get { return false; }
    }

    public virtual uint BaseTimeout {
      get { return 500; }
    }
  }

  abstract class Query<TResult> : Query where TResult : Result {
    public new TResult Result {
      get { return (TResult)base.Result; }
    }
  }
}
