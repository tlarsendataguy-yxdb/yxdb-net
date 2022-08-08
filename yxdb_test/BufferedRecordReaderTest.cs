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
            var reader = GenerateReader(getPath("LotsOfRecords.yxdb"), 5, false);

            var recordsRead = 0;
            while (reader.NextRecord())
            {
                recordsRead++;
                Assert.Equal(recordsRead, LittleEndian.ToInt32(reader.RecordBuffer, 0));
            }
            Assert.Equal(100000, recordsRead);
        }

        private BufferedRecordReader GenerateReader(string path, int fixedLen, bool hasVarFields)
        {
            var stream = File.OpenRead(path);
            var header = new byte[512];
            stream.Read(header);
            var metaInfoSize = LittleEndian.ToInt32(header, 80) * 2;
            var totalRecords = LittleEndian.ToInt32(header, 104);
            stream.Seek(metaInfoSize, SeekOrigin.Current);
            return new BufferedRecordReader(stream, fixedLen, hasVarFields, totalRecords);
        }

        private string getPath(string fileName)
        {
            return $"../../../test_files/{fileName}";
        }
    }
}