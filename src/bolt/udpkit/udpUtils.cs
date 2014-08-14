using System;

namespace UdpKit {
    internal static class UdpUtils {
        public static int EnumToInt<T> (T value) where T : struct {
            return (int) (ValueType) value;
        }

        public static T IntToEnum<T> (int value) where T : struct {
            return (T) (ValueType) value;
        }
    }
}
