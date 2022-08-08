using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb
{
    internal class LittleEndian
    {
        public static int ToInt32(byte[] value, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(value, startIndex);
            }
            return BitConverter.ToInt32(ReverseBytes(value, startIndex, 4), 0);
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