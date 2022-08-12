using System;
using System.IO;
using Xunit;
using yxdb;

namespace yxdb_test
{
    public class BufferedRecordReaderTest
    {
        [Fact]
        public void TestLotsOfRecords()
        {
            var reader = GenerateReader(GetPath("LotsOfRecords.yxdb"), 5, false);

            var recordsRead = 0;
            while (reader.NextRecord())
            {
                recordsRead++;
                Assert.Equal(recordsRead, LittleEndian.ToInt32(reader.RecordBuffer, 0));
            }
            Assert.Equal(100000, recordsRead);
            reader.Close();
        }

        [Fact]
        public void TestVeryLongFieldFile()
        {
            var reader = GenerateReader(GetPath("VeryLongField.yxdb"), 6, true);
            
            var recordsRead = 0;
            while (reader.NextRecord())
            {
                recordsRead++;
                Assert.Equal(recordsRead, reader.RecordBuffer[0]);
            }
            Assert.Equal(3, recordsRead);
            reader.Close();
        }

        private static BufferedRecordReader GenerateReader(string path, int fixedLen, bool hasVarFields)
        {
            var stream = File.OpenRead(path);
            var header = new byte[512];
            var read = stream.Read(header);
            if (read < 512)
            {
                throw new Exception("header not read");
            }
            var metaInfoSize = LittleEndian.ToInt32(header, 80) * 2;
            var totalRecords = LittleEndian.ToInt64(header, 104);
            stream.Seek(metaInfoSize, SeekOrigin.Current);
            return new BufferedRecordReader(stream, fixedLen, hasVarFields, totalRecords);
        }

        private static string GetPath(string fileName)
        {
            return $"../../../test_files/{fileName}";
        }
    }
}