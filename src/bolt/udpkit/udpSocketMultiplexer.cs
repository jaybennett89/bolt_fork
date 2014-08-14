using System.Threading;

namespace UdpKit {
    public class UdpSocketMultiplexer {
        int index;
        UdpSocket[] sockets;
        WaitHandle[] events;

        public UdpSocket[] Sockets {
            get { return sockets; }
        }

        internal UdpSocketMultiplexer (params UdpSocket[] sockets) {
            this.index = 0;
            this.sockets = sockets;
            this.events = new AutoResetEvent[this.sockets.Length];

            for (int i = 0; i < this.sockets.Length; ++i) {
                this.events[i] = this.sockets[i].EventsAvailable;
            }
        }

        public bool Wait () {
            return Wait(-1);
        }

        public bool Wait (int timeout) {
            UdpSocket socket;
            return Wait(timeout, out socket);
        }

        public bool Wait (int timeout, out UdpSocket socket) {
            int n = WaitHandle.WaitAny(this.events, timeout);

            if (n >= 0 && n < this.sockets.Length) {
                index = n;
                socket = this.sockets[n];
                return true;
            }

            socket = null;
            return false;
        }

        public bool Poll (out UdpEvent ev, out UdpSocket socket) {
            bool allowRestart = (index != 0);

        RESTART:
            for (int i = index; i < sockets.Length; ++i) {
                if (sockets[i].Poll(out ev)) {
                    index = i + 1;
                    socket = sockets[i];
                    return true;
                }
            }

            if (allowRestart) {
                index = 0;
                allowRestart = false;
                goto RESTART;
            }

            ev = default(UdpEvent);
            socket = null;
            return false;
        }
    }
}