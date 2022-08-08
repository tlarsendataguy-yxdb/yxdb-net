using System;
using System.Text;
using Xunit;
using yxdb;

namespace yxdb_test
{
    public class ExtractorsTest
    {
        [Fact]
        public void ExtractInt16()
        {
            var extract = Extractors.NewInt16Extractor(2);
            var result = extract(new byte[] { 0, 0, 10, 0, 0, 0 });

            Assert.Equal(10, result);
        }

        [Fact]
        public void ExtractNullInt16()
        {
            var extract = Extractors.NewInt16Extractor(2);
            var result = extract(new byte[] { 0, 0, 10, 0, 1, 0 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractInt32()
        {
            var extract = Extractors.NewInt32Extractor(3);
            var result = extract(new byte[] { 0, 0, 0, 10, 0, 0, 0, 0 });

            Assert.Equal(10, result);
        }

        [Fact]
        public void ExtractNullInt32()
        {
            var extract = Extractors.NewInt32Extractor(3);
            var result = extract(new byte[] { 0, 0, 0, 10, 0, 0, 0, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractInt64()
        {
            var extract = Extractors.NewInt64Extractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0 });

            Assert.Equal(10, result);
        }

        [Fact]
        public void ExtractNullInt64()
        {
            var extract = Extractors.NewInt64Extractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractBool()
        {
            var extract = Extractors.NewBoolExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 });

            Assert.Equal(true, result);
        }

        [Fact]
        public void ExtractNullBool()
        {
            var extract = Extractors.NewBoolExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractByte()
        {
            var extract = Extractors.NewByteExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 0, 0, 0 });

            Assert.Equal((byte)10, result);
        }

        [Fact]
        public void ExtractNullByte()
        {
            var extract = Extractors.NewByteExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 2, 1, 0, 0, 0, 0, 0, 0, 0 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractFloat()
        {
            var extract = Extractors.NewFloatExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 205, 206, 140, 63, 0, 0, 0, 0, 0 });

            Assert.Equal(BitConverter.ToSingle(new byte[] { 205, 206, 140, 63 }), result);
        }

        [Fact]
        public void ExtractNullFloat()
        {
            var extract = Extractors.NewFloatExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 205, 206, 140, 63, 1, 0, 0, 0, 0 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractDouble()
        {
            var extract = Extractors.NewDoubleExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 154, 155, 155, 155, 155, 155, 241, 63, 0 });

            Assert.Equal(BitConverter.ToDouble(new byte[] { 154, 155, 155, 155, 155, 155, 241, 63 }), result);
        }

        [Fact]
        public void ExtractNullDouble()
        {
            var extract = Extractors.NewDoubleExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 154, 155, 155, 155, 155, 155, 241, 63, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractDate()
        {
            var extract = Extractors.NewDateExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 50, 48, 50, 49, 45, 48, 49, 45, 48, 49, 0 });

            Assert.Equal(new DateTime(2021, 1, 1), result);
        }

        [Fact]
        public void ExtractNullDate()
        {
            var extract = Extractors.NewDateExtractor(4);
            var result = extract(new byte[] { 0, 0, 0, 0, 50, 48, 50, 49, 45, 48, 49, 45, 48, 49, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractDateTime()
        {
            var extract = Extractors.NewDateTimeExtractor(4);
            var result = extract(new byte[]
                { 0, 0, 0, 0, 50, 48, 50, 49, 45, 48, 49, 45, 48, 50, 32, 48, 51, 58, 48, 52, 58, 48, 53, 0 });

            Assert.Equal(new DateTime(2021, 1, 2, 3, 4, 5), result);
        }

        [Fact]
        public void ExtractNullDateTime()
        {
            var extract = Extractors.NewDateTimeExtractor(4);
            var result = extract(new byte[]
                { 0, 0, 0, 0, 50, 48, 50, 49, 45, 48, 49, 45, 48, 50, 32, 48, 51, 58, 48, 52, 58, 48, 53, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractString()
        {
            var extract = Extractors.NewStringExtractor(2, 15);
            var result = extract(new byte[]
                { 0, 0, 104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 33, 0, 23, 77, 0 });

            Assert.Equal("hello world!", result);
        }

        [Fact]
        public void ExtractFullString()
        {
            var extract = Extractors.NewStringExtractor(2, 5);
            var result = extract(new byte[] { 0, 0, 104, 101, 108, 108, 111, 0 });

            Assert.Equal("hello", result);
        }

        [Fact]
        public void ExtractNullString()
        {
            var extract = Extractors.NewStringExtractor(2, 5);
            var result = extract(new byte[] { 0, 0, 104, 101, 108, 108, 111, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractEmptyString()
        {
            var extract = Extractors.NewStringExtractor(2, 5);
            var result = extract(new byte[] { 0, 0, 0, 101, 108, 108, 111, 0 });

            Assert.Equal("", result);
        }

        [Fact]
        public void ExtractFixedDecimal()
        {
            var extract = Extractors.NewFixedDecimalExtractor(2, 10);
            var result = extract(new byte[] { 0, 0, 49, 50, 51, 46, 52, 53, 0, 43, 67, 110, 0 });

            Assert.Equal(123.45, result);
        }

        [Fact]
        public void ExtractNullFixedDecimal()
        {
            var extract = Extractors.NewFixedDecimalExtractor(2, 10);
            var result = extract(new byte[] { 0, 0, 49, 50, 51, 46, 52, 53, 0, 43, 67, 110, 1 });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractWString()
        {
            var extract = Extractors.NewWStringExtractor(2, 15);
            var result = extract(new byte[]
            {
                0, 0, 104, 0, 101, 0, 108, 0, 108, 0, 111, 0, 32, 0, 119, 0, 111, 0, 114, 0, 108, 0, 100, 0, 0, 0, 12,
                0, 44, 0, 55, 0, 0
            });

            Assert.Equal("hello world", result);
        }

        [Fact]
        public void ExtractNullWString()
        {
            var extract = Extractors.NewWStringExtractor(2, 15);
            var result = extract(new byte[]
            {
                0, 0, 104, 0, 101, 0, 108, 0, 108, 0, 111, 0, 32, 0, 119, 0, 111, 0, 114, 0, 108, 0, 100, 0, 0, 0, 12,
                0, 44, 0, 55, 0, 1
            });

            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractEmptyWString()
        {
            var extract = Extractors.NewWStringExtractor(2, 15);
            var result = extract(new byte[]
            {
                0, 0, 0, 0, 101, 0, 108, 0, 108, 0, 111, 0, 32, 0, 119, 0, 111, 0, 114, 0, 108, 0, 100, 0, 0, 0, 12, 0,
                44, 0, 55, 0, 0
            });

            Assert.Equal("", result);
        }

        [Fact]
        public void ExtractNormalBlob()
        {
            var extract = Extractors.NewBlobExtractor(6);
            var result = extract(_normalBlob);
            var expected = Encoding.ASCII.GetBytes(new string('B', 200));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractSmallBlob()
        {
            var extract = Extractors.NewBlobExtractor(6);
            var result = extract(_smallBlob);
            var expected = Encoding.ASCII.GetBytes(new string('B', 100));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ExtractTinyBlob()
        {
            var extract = Extractors.NewBlobExtractor(6);
            var result = extract(new byte[] { 1, 0, 65, 0, 0, 32, 66, 0, 0, 16, 0, 0, 0, 0 });
            Assert.Equal(new byte[]{66}, result);
        }

        [Fact]
        public void ExtractEmptyBlob()
        {
            var extract = Extractors.NewBlobExtractor(6);
            var result = extract(new byte[] { 1, 0, 65, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0 });
            Assert.Equal(new byte[]{}, result);
        }

        [Fact]
        public void ExtractNullBlob()
        {
            var extract = Extractors.NewBlobExtractor(6);
            var result = extract(new byte[] { 1, 0, 65, 0, 0, 32, 1, 0, 0, 0, 0, 0, 0, 0 });
            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractV_String()
        {
            var extract = Extractors.NewV_StringExtractor(6);
            var result = extract(_smallBlob);
            Assert.Equal(new string('B', 100), result);
        }

        [Fact]
        public void ExtractNullV_String()
        {
            var extract = Extractors.NewV_StringExtractor(2);
            var result = extract(new byte[] { 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractEmptyV_String()
        {
            var extract = Extractors.NewV_StringExtractor(2);
            var result = extract(new byte[] { 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            Assert.Equal("", result);
        }

        [Fact]
        public void ExtractV_WString()
        {
            var extract = Extractors.NewV_WStringExtractor(2);
            var result = extract(_normalBlob);
            Assert.Equal(new string('A', 100), result);
        }

        [Fact]
        public void ExtractNullV_WString()
        {
            var extract = Extractors.NewV_WStringExtractor(2);
            var result = extract(new byte[] { 0, 0, 1, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            Assert.Equal(null, result);
        }

        [Fact]
        public void ExtractEmptyV_WString()
        {
            var extract = Extractors.NewV_WStringExtractor(2);
            var result = extract(new byte[] { 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 });
            Assert.Equal("", result);
        }

        private static byte[] _normalBlob = new byte[]
        {
            1, 0, 12, 0, 0, 0, 212, 0, 0, 0, 152, 1, 0, 0, 144, 1, 0, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65,
            0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65,
            0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65,
            0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65,
            0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 144, 1, 0, 0, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66
        };

        private static byte[] _smallBlob = new byte[]
        {
            1, 0, 12, 0, 0, 0, 109, 0, 0, 0, 202, 0, 0, 0, 201, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65,
            0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0,
            65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 65, 0, 201, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66,
            66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66, 66
        };
    }
}