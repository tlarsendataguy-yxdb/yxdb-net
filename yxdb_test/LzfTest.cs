using System;
using Xunit;
using yxdb;

namespace yxdb_test
{
    public class LzfTest
    {
        [Fact]
        public void TestEmptyInput()
        {
            var inData = new byte[]{};
            var outData = new byte[]{};
            var lzf = new Lzf(inData, outData);

            var written = lzf.Decompress(0);
            Assert.Equal(0, written);
        }

        [Fact]
        public void OutputArrayIsTooSmall()
        {
            var inData = new byte[] { 0, 25 };
            var outData = new byte[] { };
            var lzf = new Lzf(inData, outData);
            
            Assert.Throws<ArgumentException>(()=> lzf.Decompress(2));
        }

        [Fact]
        public void SmallControlValuesDoSimpleCopies()
        {
            var inData = new byte[] { 4, 1, 2, 3, 4, 5 };
            var outData = new byte[5];
            var lzf = new Lzf(inData, outData);

            var written = lzf.Decompress(6);
            Assert.Equal(5, written);
            Assert.Equal(new byte[]{1,2,3,4,5}, outData);
        }

        [Fact]
        public void MultipleSmallControlValues()
        {
            var inData = new byte[] { 2, 1, 2, 3, 1, 1, 2 };
            var outData = new byte[5];
            var lzf = new Lzf(inData, outData);

            var written = lzf.Decompress(7);
            Assert.Equal(5, written);
            Assert.Equal(new byte[]{1,2,3,1,2}, outData);
        }

        [Fact]
        public void ExpandLargeControlValues()
        {
            var inData = new byte[] { 2, 1, 2, 3, 32, 1 };
            var outData = new byte[6];
            var lzf = new Lzf(inData, outData);

            var written = lzf.Decompress(6);
            Assert.Equal(6, written);
            Assert.Equal(new byte[]{1,2,3,2,3,2}, outData);
        }

        [Fact]
        public void LargeControlValuesWithLengthOf7()
        {
            var inData = new byte[] { 8, 1, 2, 3, 4, 5, 6, 7, 8, 9, 224, 1, 8 };
            var outData = new byte[19];
            var lzf = new Lzf(inData, outData);

            var written = lzf.Decompress(13);
            Assert.Equal(19, written);
            Assert.Equal(new byte[]{1,2,3,4,5,6,7,8,9,1,2,3,4,5,6,7,8,9,1}, outData);
        }

        [Fact]
        public void OutputArrayTooSmallForLargeControlValues()
        {
            var inData = new byte[]{8, 1, 2, 3, 4, 5, 6, 7, 8, 9, 224, 1, 8};
            var outData = new byte[17];
            var lzf = new Lzf(inData, outData);

            Assert.Throws<ArgumentException>(() =>lzf.Decompress(13));
        }

        [Fact]
        public void ResetLzfAndStartAgain()
        {
            var inData = new byte[]{4, 1, 2, 3, 4, 5};
            var outData = new byte[5];
            var lzf = new Lzf(inData, outData);

            lzf.Decompress(6);

            inData[0] = 2;
            inData[1] = 6;
            inData[2] = 7;
            inData[3] = 8;

            var written = lzf.Decompress(4);
            Assert.Equal(3, written);
            Assert.Equal(new byte[]{6, 7, 8, 4, 5}, outData);
        }
    }
}