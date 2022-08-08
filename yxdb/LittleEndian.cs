using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb
{
    internal static class LittleEndian
    {
        public static short ToInt16(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt16(value, startIndex);
            }
            return BitConverter.ToInt16(ReverseBytes(value, startIndex, 2), 0);
        }

        public static int ToInt32(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(value, startIndex);
            }
            return BitConverter.ToInt32(ReverseBytes(value, startIndex, 4), 0);
        }

        public static long ToInt64(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt64(value, startIndex);
            }
            return BitConverter.ToInt64(ReverseBytes(value, startIndex, 8), 0);
        }

        private static byte[] ReverseBytes(byte[] value, int startIndex, int size)
        {
            var bytes = new byte[size];
            Array.Copy(value, startIndex, bytes, 0, size);
            Array.Reverse(bytes);
            return bytes;
        }
    }
}