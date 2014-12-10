using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UdpKit {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UdpEndPoint : IEquatable<UdpEndPoint>, IComparable<UdpEndPoint> {
        public class Comparer : IEqualityComparer<UdpEndPoint> {
            bool IEqualityComparer<UdpEndPoint>.Equals (UdpEndPoint x, UdpEndPoint y) {
                return UdpEndPoint.Compare(x, y) == 0;
            }

            int IEqualityComparer<UdpEndPoint>.GetHashCode (UdpEndPoint obj) {
                return obj.GetHashCode();
            }
        }

        public static readonly UdpEndPoint Any = new UdpEndPoint(UdpIPv4Address.Any, 0);

        public readonly UdpIPv4Address Address;
        public readonly ushort Port;

        public UdpEndPoint (UdpIPv4Address address, ushort port) {
            this.Address = address;
            this.Port = port;
        }

        public int CompareTo (UdpEndPoint other) {
            return Compare(this, other);
        }

        public bool Equals (UdpEndPoint other) {
            return Compare(this, other) == 0;
        }

        public override int GetHashCode () {
            return (int) (Address.Packed ^ Port);
        }

        public override bool Equals (object obj) {
            if (obj is UdpEndPoint) {
                return Compare(this, (UdpEndPoint) obj) == 0;
            }

            return false;
        }

        public override string ToString () {
            return string.Format("[EndPoint {0}.{1}.{2}.{3}:{4}]", Address.Byte3, Address.Byte2, Address.Byte1, Address.Byte0, Port);
        }

        public static UdpEndPoint Parse (string endpoint) {
            string[] parts = endpoint.Split(':');

            if (parts.Length != 2) { 
                throw new FormatException("endpoint is not in the correct format");
            }
            
            UdpIPv4Address address = UdpIPv4Address.Parse(parts[0]);
            return new UdpEndPoint(address, ushort.Parse(parts[1]));
        }

        public static bool operator == (UdpEndPoint x, UdpEndPoint y) {
            return Compare(x, y) == 0;
        }

        public static bool operator != (UdpEndPoint x, UdpEndPoint y) {
            return Compare(x, y) != 0;
        }

        static int Compare (UdpEndPoint x, UdpEndPoint y) {
            int cmp = x.Address.CompareTo(y.Address);

            if (cmp == 0) {
                cmp = x.Port.CompareTo(y.Port);
            }

            return cmp;
        }
    }
}
