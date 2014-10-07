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
			sb.Append(" HAS_CONTROL");
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
    public static readonly ProxyFlags CREATE_DONE = new ProxyFlags(2);
    public static readonly ProxyFlags DESTROY_REQUESTED = new ProxyFlags(4);
    public static readonly ProxyFlags DESTROY_PENDING = new ProxyFlags(8);
    public static readonly ProxyFlags IDLE = new ProxyFlags(16);
    public static readonly ProxyFlags FORCE_SYNC = new ProxyFlags(32);
    
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
			sb.Append(" CREATE_DONE");
		}
			if((bits & 4) == 4) {
			sb.Append(" DESTROY_REQUESTED");
		}
			if((bits & 8) == 8) {
			sb.Append(" DESTROY_PENDING");
		}
			if((bits & 16) == 16) {
			sb.Append(" IDLE");
		}
			if((bits & 32) == 32) {
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


public struct CommandFlags {
    public static readonly CommandFlags ZERO = new CommandFlags(0);
    public static readonly CommandFlags HAS_EXECUTED = new CommandFlags(1);
    public static readonly CommandFlags SEND_STATE = new CommandFlags(2);
    public static readonly CommandFlags SEND_STATE_PERFORMED = new CommandFlags(4);
    public static readonly CommandFlags CORRECTION_RECEIVED = new CommandFlags(8);
    public static readonly CommandFlags DISPOSE = new CommandFlags(16);
    
    readonly int bits;

    public bool IsZero {
      get { return bits == 0; }
    }

    CommandFlags (int val) {
      bits = val;
    }
	
    public override int GetHashCode() {
      return bits;
    }

    public override bool Equals(object obj) {
      if (obj is CommandFlags) {
        return bits == ((CommandFlags)obj).bits;
      }

      return false;
    }

    public override string ToString() {
	  System.Text.StringBuilder sb = new System.Text.StringBuilder ();
	  sb.Append("[");
	  sb.Append("CommandFlags");

	  
    		if((bits & 1) == 1) {
			sb.Append(" HAS_EXECUTED");
		}
			if((bits & 2) == 2) {
			sb.Append(" SEND_STATE");
		}
			if((bits & 4) == 4) {
			sb.Append(" SEND_STATE_PERFORMED");
		}
			if((bits & 8) == 8) {
			sb.Append(" CORRECTION_RECEIVED");
		}
			if((bits & 16) == 16) {
			sb.Append(" DISPOSE");
		}
	
	  sb.Append("]");
	  return sb.ToString();
    }

    public static implicit operator bool (CommandFlags a) {
      return a.bits != 0;
    }

    public static explicit operator int (CommandFlags a) {
      return a.bits;
    }
	
    public static explicit operator CommandFlags (int a) {
      return new CommandFlags(a);
    }

    public static CommandFlags operator & (CommandFlags a, CommandFlags b) {
      return new CommandFlags(a.bits & b.bits);
    }

    public static CommandFlags operator | (CommandFlags a, CommandFlags b) {
      return new CommandFlags(a.bits | b.bits);
    }

    public static CommandFlags operator ^ (CommandFlags a, CommandFlags b) {
      return new CommandFlags(a.bits ^ b.bits);
    }

    public static CommandFlags operator ~ (CommandFlags a) {
      return new CommandFlags(~a.bits);
    }
	
    public static bool operator ==(CommandFlags a, CommandFlags b) {
      return a.bits == b.bits;
    }

    public static bool operator !=(CommandFlags a, CommandFlags b) {
      return a.bits != b.bits;
    }
  }

}