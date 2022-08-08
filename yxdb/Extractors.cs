using System.Text;
using System;
using System.Globalization;

namespace yxdb
{
    internal static class Extractors
    {
        public static Func<byte[], bool?> NewBoolExtractor(int start)
        {
            return (buffer) =>
            {
                var value = buffer[start];
                if (value == 2)
                {
                    return null;
                }

                return value == 1;
            };
        }

        public static Func<byte[], byte?> NewByteExtractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 1] == 1)
                {
                    return null;
                }

                return buffer[start];
            };
        }

        public static Func<byte[], long?> NewInt16Extractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 2] == 1)
                {
                    return null;
                }

                return LittleEndian.ToInt16(buffer, start);
            };
        }

        public static Func<byte[], long?> NewInt32Extractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 4] == 1)
                {
                    return null;
                }

                return LittleEndian.ToInt32(buffer, start);
            };
        }

        public static Func<byte[], long?> NewInt64Extractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 8] == 1)
                {
                    return null;
                }

                return LittleEndian.ToInt64(buffer, start);
            };
        }

        public static Func<byte[], double?> NewFixedDecimalExtractor(int start, int fieldLength)
        {
            return (buffer) =>
            {
                if (buffer[start + fieldLength] == 1)
                {
                    return null;
                }

                var str = GetString(buffer, start, fieldLength, 1);
                return double.Parse(str);
            };
        }

        public static Func<byte[], double?> NewFloatExtractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 4] == 1)
                {
                    return null;
                }

                return BitConverter.ToSingle(buffer, start); // not sure if we should be checking endian-ness here
            };
        }

        public static Func<byte[], double?> NewDoubleExtractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 8] == 1)
                {
                    return null;
                }

                return BitConverter.ToDouble(buffer, start); // not sure if we should be checking endian-ness here
            };
        }

        public static Func<byte[], DateTime?> NewDateExtractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 10] == 1)
                {
                    return null;
                }

                return ParseDate(buffer, start, 10, "yyyy-MM-dd");
            };
        }

        public static Func<byte[], DateTime?> NewDateTimeExtractor(int start)
        {
            return (buffer) =>
            {
                if (buffer[start + 19] == 1)
                {
                    return null;
                }

                return ParseDate(buffer, start, 19, "yyyy-MM-dd HH:mm:ss");
            };
        }

        public static Func<byte[], string> NewStringExtractor(int start, int fieldLen)
        {
            return (buffer) =>
            {
                if (buffer[start + fieldLen] == 1)
                {
                    return null;
                }

                return GetString(buffer, start, fieldLen, 1);
            };
        }

        public static Func<byte[], string> NewWStringExtractor(int start, int fieldLength)
        {
            return (buffer) =>
            {
                if (buffer[start + (fieldLength * 2)] == 1)
                {
                    return null;
                }

                return GetString(buffer, start, fieldLength, 2);
            };
        }

        public static Func<byte[], string> NewV_StringExtractor(int start)
        {
            return (buffer) =>
            {
                var bytes = ParseBlob(buffer, start);
                if (bytes == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            };
        }

        public static Func<byte[], string> NewV_WStringExtractor(int start)
        {
            return (buffer) =>
            {
                var bytes = ParseBlob(buffer, start);
                if (bytes == null)
                {
                    return null;
                }

                return Encoding.Unicode.GetString(bytes, 0, bytes.Length);
            };
        }

        public static Func<byte[], byte[]> NewBlobExtractor(int start)
        {
            return (buffer) => ParseBlob(buffer, start);
        }

        private static DateTime ParseDate(byte[] buffer, int start, int len, string format)
        {
            var str = GetString(buffer, start, len, 1);
            return DateTime.ParseExact(str, format, CultureInfo.InvariantCulture);
        }

        private static string GetString(byte[] buffer, int start, int fieldLength, int charSize)
        {
            var len = GetStrLen(buffer, start, fieldLength, charSize);
            if (charSize == 1)
            {
                return Encoding.UTF8.GetString(buffer, start, len);
            }

            return Encoding.Unicode.GetString(buffer, start, len);
        }

        private static int GetStrLen(byte[] buffer, int start, int fieldLength, int charSize)
        {
            int fieldTo = start + (fieldLength * charSize);
            int strLen = 0;
            for (var i = start; i < fieldTo; i += charSize)
            {
                if (buffer[i] == 0 && buffer[i + (charSize - 1)] == 0)
                {
                    break;
                }

                strLen++;
            }

            return strLen;
        }

        private static byte[] ParseBlob(byte[] buffer, int start)
        {
            var fixedPortion = LittleEndian.ToInt32(buffer, start);
            if (fixedPortion == 0)
            {
                return new byte[] { };
            }

            if (fixedPortion == 1)
            {
                return null;
            }

            if (IsTiny(fixedPortion))
            {
                return GetTinyBlob(start, buffer);
            }

            var blockStart = start + (fixedPortion & 0x7fffffff);
            var blockFirstByte = buffer[blockStart];
            if (IsSmallBlock(blockFirstByte))
            {
                return GetSmallBlob(buffer, blockStart);
            }

            return GetNormalBlob(buffer, blockStart);
        }

        private static bool IsTiny(int fixedPortion)
        {
            var bitCheck1 = fixedPortion & 0x80000000;
            var bitCheck2 = fixedPortion & 0x30000000;
            return bitCheck1 == 0 && bitCheck2 != 0;
        }

        private static byte[] GetTinyBlob(int start, byte[] buffer)
        {
            var intVal = LittleEndian.ToInt32(buffer, start);
            var len = intVal >> 28;
            var blob = new byte[len];
            Array.Copy(buffer, start, blob, 0, len);
            return blob;
        }

        private static bool IsSmallBlock(byte value)
        {
            return (value & 1) == 1;
        }

        private static byte[] GetSmallBlob(byte[] buffer, int blockStart)
        {
            var blockFirstByte = buffer[blockStart];
            var blobLen = blockFirstByte >> 1;
            var blobStart = blockStart + 1;
            var blob = new byte[blobLen];
            Array.Copy(buffer, blobStart, blob, 0, blobLen);
            return blob;
        }

        private static byte[] GetNormalBlob(byte[] buffer, int blockStart)
        {
            var blobLen = LittleEndian.ToInt32(buffer, blockStart) / 2;
            var blobStart = blockStart + 4;
            var blob = new byte[blobLen];
            Array.Copy(buffer, blobStart, blob, 0, blobLen);
            return blob;
        }
    }
}