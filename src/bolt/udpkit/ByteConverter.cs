using System;
using System.Runtime.InteropServices;

namespace UdpKit {
    [StructLayout(LayoutKind.Explicit)]
    public struct UdpByteConverter {
        [FieldOffset(0)]
        public Int16 Signed16;
        [FieldOffset(0)]
        public UInt16 Unsigned16;
        [FieldOffset(0)]
        public Char Char;
        [FieldOffset(0)]
        public Int32 Signed32;
        [FieldOffset(0)]
        public UInt32 Unsigned32;
        [FieldOffset(0)]
        public Int64 Signed64;
        [FieldOffset(0)]
        public UInt64 Unsigned64;
        [FieldOffset(0)]
        public Single Float32;
        [FieldOffset(0)]
        public Double Float64;

        [FieldOffset(0)]
        public Byte Byte0; 
        [FieldOffset(1)]
        public Byte Byte1;
        [FieldOffset(2)]
        public Byte Byte2;
        [FieldOffset(3)]
        public Byte Byte3;
        [FieldOffset(4)]
        public Byte Byte4;
        [FieldOffset(5)]
        public Byte Byte5;
        [FieldOffset(6)]
        public Byte Byte6;
        [FieldOffset(7)]
        public Byte Byte7;

        public static implicit operator UdpByteConverter (Int16 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Signed16 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (UInt16 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Unsigned16 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (Char val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Char = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (UInt32 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Unsigned32 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (Int32 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Signed32 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (UInt64 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Unsigned64 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (Int64 val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Signed64 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (Single val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Float32 = val;
            return bytes;
        }

        public static implicit operator UdpByteConverter (Double val) {
            UdpByteConverter bytes = default(UdpByteConverter);
            bytes.Float64 = val;
            return bytes;
        }
    }
}