using System.Collections.Generic;

namespace UdpKit {
    public delegate UdpSerializer UdpSerializerFactory ();

    public abstract class UdpSerializer {
        readonly Queue<object> sendQueue = new Queue<object>(32);

        internal bool HasQueuedObjects {
            get { return sendQueue.Count > 0; }
        }

        internal object NextObject () {
            return sendQueue.Dequeue();
        }

        /// <summary>
        /// The connection which owns this serializer
        /// </summary>
        public UdpConnection Connection {
            get;
            internal set;
        }

        /// <summary>
        /// Queue an object for immediate sending after the current object has been packed
        /// </summary>
        /// <param name="o">The object to send</param>
        public void SendNext (object o) {
            sendQueue.Enqueue(o);
        }

        /// <summary>
        /// Reject an object from being sent, sending it back to the event-thread
        /// </summary>
        /// <param name="o">The object to reject</param>
        public void Reject (object o) {
            Connection.socket.Raise(UdpEvent.PUBLIC_OBJECT_REJECTED, Connection, o);
        }

        public abstract bool Pack (UdpStream stream, ref object o);
        public abstract bool Unpack (UdpStream stream, ref object o);
    }

    public abstract class UdpSerializer<T> : UdpSerializer {
        public sealed override bool Pack (UdpStream stream, ref object o) {
            T sent;
            bool result = Pack(stream, (T) o, out sent);
            o = sent;
            return result;
        }

        public sealed override bool Unpack (UdpStream stream, ref object o) {
            T received;
            bool result = Unpack(stream, out received);
            o = received;
            return result;
        }

        public abstract bool Pack (UdpStream stream, T input, out T sent);
        public abstract bool Unpack (UdpStream stream, out T received);
    }
}
