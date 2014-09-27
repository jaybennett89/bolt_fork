using UdpKit;

namespace Bolt {


public struct EntityFlags {
    public static readonly EntityFlags ZERO = new EntityFlags(0);
    public static readonly EntityFlags HAS_CONTROL = new EntityFlags(1);
    public static readonly EntityFlags PERSIST_ON_LOAD = new EntityFlags(2);
    
    readonly int bits;

    public bool IsZero {
      get { return bits == 0; }
    }

    EntityFlags (int val) {
      bits = val;
    }
	
    public override int GetHashCode() {
      return bits;
    }

    public override bool Equals(object obj) {
      if (obj is EntityFlags) {
        return bits == ((EntityFlags)obj).bits;
      }

      return false;
    }

    public override string ToString() {
	  System.Text.StringBuilder sb = new System.Text.StringBuilder ();
	  sb.Append("[");
	  sb.Append("EntityFlags");

	  
    		if((bits & 1) == 1) {
			sb.Append(" CONTROLLING");
		}
			if((bits & 2) == 2) {
			sb.Append(" PERSIST_ON_LOAD");
		}
	
	  sb.Append("]");
	  return sb.ToString();
    }

    public static implicit operator bool (EntityFlags a) {
      return a.bits != 0;
    }

    public static explicit operator int (EntityFlags a) {
      return a.bits;
    }
	
    public static explicit operator EntityFlags (int a) {
      return new EntityFlags(a);
    }

    public static EntityFlags operator & (EntityFlags a, EntityFlags b) {
      return new EntityFlags(a.bits & b.bits);
    }

    public static EntityFlags operator | (EntityFlags a, EntityFlags b) {
      return new EntityFlags(a.bits | b.bits);
    }

    public static EntityFlags operator ^ (EntityFlags a, EntityFlags b) {
      return new EntityFlags(a.bits ^ b.bits);
    }

    public static EntityFlags operator ~ (EntityFlags a) {
      return new EntityFlags(~a.bits);
    }
	
    public static bool operator ==(EntityFlags a, EntityFlags b) {
      return a.bits == b.bits;
    }

    public static bool operator !=(EntityFlags a, EntityFlags b) {
      return a.bits != b.bits;
    }
  }


public struct ProxyFlags {
    public static readonly ProxyFlags ZERO = new ProxyFlags(0);
    public static readonly ProxyFlags CREATE_REQUESTED = new ProxyFlags(1);
    public static readonly ProxyFlags CREATE_IN_PROGRESS = new ProxyFlags(2);
    public static readonly ProxyFlags CREATE_DONE = new ProxyFlags(4);
    public static readonly ProxyFlags DESTROY_REQUESTED = new ProxyFlags(8);
    public static readonly ProxyFlags DESTROY_IN_PROGRESS = new ProxyFlags(16);
    public static readonly ProxyFlags DESTROY_DONE = new ProxyFlags(32);
    public static readonly ProxyFlags IDLE = new ProxyFlags(64);
    public static readonly ProxyFlags FORCE_SYNC = new ProxyFlags(128);
    
    readonly int bits;

    public bool IsZero {
      get { return bits == 0; }
    }

    ProxyFlags (int val) {
      bits = val;
    }
	
    public override int GetHashCode() {
      return bits;
    }

    public override bool Equals(object obj) {
      if (obj is ProxyFlags) {
        return bits == ((ProxyFlags)obj).bits;
      }

      return false;
    }

    public override string ToString() {
	  System.Text.StringBuilder sb = new System.Text.StringBuilder ();
	  sb.Append("[");
	  sb.Append("ProxyFlags");

	  
    		if((bits & 1) == 1) {
			sb.Append(" CREATE_REQUESTED");
		}
			if((bits & 2) == 2) {
			sb.Append(" CREATE_IN_PROGRESS");
		}
			if((bits & 4) == 4) {
			sb.Append(" CREATE_DONE");
		}
			if((bits & 8) == 8) {
			sb.Append(" DESTROY_REQUESTED");
		}
			if((bits & 16) == 16) {
			sb.Append(" DESTROY_IN_PROGRESS");
		}
			if((bits & 32) == 32) {
			sb.Append(" DESTROY_DONE");
		}
			if((bits & 64) == 64) {
			sb.Append(" IDLE");
		}
			if((bits & 128) == 128) {
			sb.Append(" FORCE_SYNC");
		}
	
	  sb.Append("]");
	  return sb.ToString();
    }

    public static implicit operator bool (ProxyFlags a) {
      return a.bits != 0;
    }

    public static explicit operator int (ProxyFlags a) {
      return a.bits;
    }
	
    public static explicit operator ProxyFlags (int a) {
      return new ProxyFlags(a);
    }

    public static ProxyFlags operator & (ProxyFlags a, ProxyFlags b) {
      return new ProxyFlags(a.bits & b.bits);
    }

    public static ProxyFlags operator | (ProxyFlags a, ProxyFlags b) {
      return new ProxyFlags(a.bits | b.bits);
    }

    public static ProxyFlags operator ^ (ProxyFlags a, ProxyFlags b) {
      return new ProxyFlags(a.bits ^ b.bits);
    }

    public static ProxyFlags operator ~ (ProxyFlags a) {
      return new ProxyFlags(~a.bits);
    }
	
    public static bool operator ==(ProxyFlags a, ProxyFlags b) {
      return a.bits == b.bits;
    }

    public static bool operator !=(ProxyFlags a, ProxyFlags b) {
      return a.bits != b.bits;
    }
  }

}