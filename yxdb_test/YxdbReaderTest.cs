using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using yxdb;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace yxdb_test
{
    public class YxdbReaderTest
    {
	    private readonly ITestOutputHelper _output;
	    public YxdbReaderTest(ITestOutputHelper output)
	    {
		    this._output = output;
	    }
	    
        [Fact]
        public void TestGetReader()
        {
            var path = GetPath("AllNormalFields.yxdb");
            var yxdb = new YxdbReader(path);
            Assert.Equal(1, yxdb.NumRecords);
            Assert.NotNull(yxdb.MetaInfoStr);
            Assert.Equal(AllNormalFieldsMetaXml, yxdb.MetaInfoStr);
            Assert.Equal(16, yxdb.ListFields().Count);

            int read = 0;
            while (yxdb.Next())
            {
	            Assert.Equal((byte)1, yxdb.ReadByte(0));
	            Assert.Equal((byte)1, yxdb.ReadByte("ByteField"));
	            Assert.True(yxdb.ReadBool(1));
	            Assert.True(yxdb.ReadBool("BoolField"));
	            Assert.Equal(16, yxdb.ReadLong(2));
	            Assert.Equal(16, yxdb.ReadLong("Int16Field"));
	            Assert.Equal(32, yxdb.ReadLong(3));
	            Assert.Equal(32, yxdb.ReadLong("Int32Field"));
	            Assert.Equal(64, yxdb.ReadLong(4));
	            Assert.Equal(64, yxdb.ReadLong("Int64Field"));
	            Assert.Equal(123.45, yxdb.ReadDouble(5));
	            Assert.Equal(123.45, yxdb.ReadDouble("FixedDecimalField"));
	            Assert.Equal("A", yxdb.ReadString(8));
	            Assert.Equal("A", yxdb.ReadString("StringField"));
	            Assert.Equal("AB", yxdb.ReadString(9));
	            Assert.Equal("AB", yxdb.ReadString("WStringField"));
	            Assert.Equal("ABC", yxdb.ReadString(10));
	            Assert.Equal("ABC", yxdb.ReadString("V_StringShortField"));
	            Assert.Equal(new string('B', 500), yxdb.ReadString(11));
	            Assert.Equal(new string('B', 500), yxdb.ReadString("V_StringLongField"));
	            Assert.Equal("XZY", yxdb.ReadString(12));
	            Assert.Equal("XZY", yxdb.ReadString("V_WStringShortField"));
	            Assert.Equal(new string('W', 500), yxdb.ReadString(13));
	            Assert.Equal(new string('W', 500), yxdb.ReadString("V_WStringLongField"));

	            var expected = new DateTime(2020, 1, 1);
	            Assert.Equal(expected, yxdb.ReadDate(14));
	            Assert.Equal(expected, yxdb.ReadDate("DateField"));

	            expected = new DateTime(2020, 2, 3, 4, 5, 6);
	            Assert.Equal(expected, yxdb.ReadDate(15));
	            Assert.Equal(expected, yxdb.ReadDate("DateTimeField"));
	            
	            read++;
            }
            Assert.Equal(1, read);
            yxdb.Close();
        }

        [Fact]
        public void TestLotsOfRecords()
        {
	        var path = GetPath("LotsOfRecords.yxdb");
	        var yxdb = new YxdbReader(path);

	        long? sum = 0;
	        while (yxdb.Next())
	        {
		        sum += yxdb.ReadLong(0);
	        }
	        Assert.Equal(5000050000, sum);
	        yxdb.Close();
        }

        [Fact]
        public void TestLoadReaderFromStream()
        {
	        var path = GetPath("LotsOfRecords.yxdb");
	        var stream = new FileStream(path, FileMode.Open);
	        var yxdb = new YxdbReader(stream);

	        long? sum = 0;
	        while (yxdb.Next())
	        {
		        sum += yxdb.ReadLong(0);
	        }
	        Assert.Equal(5000050000, sum);
	        yxdb.Close();
        }

        [Fact]
        public void RetrievingFieldWithWrongTypeThrows()
        {
	        var path = GetPath("AllNormalFields.yxdb");
	        var yxdb = new YxdbReader(path);
	        yxdb.Next();
	        Assert.Throws<ArgumentException>(() => yxdb.ReadString(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadBool(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadBlob(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadDate(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadDouble(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadLong(0));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadByte(1));
	        try
	        {
		        yxdb.ReadString(0);
	        }
	        catch (Exception ex)
	        {
		        _output.WriteLine(ex.ToString());
	        }
	        yxdb.Close();
        }

        [Fact]
        public void RetrievingFieldWithInvalidNameThrows()
        {
	        var path = GetPath("AllNormalFields.yxdb");
	        var yxdb = new YxdbReader(path);
	        yxdb.Next();
	        Assert.Throws<ArgumentException>(() => yxdb.ReadString("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadBool("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadBlob("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadDate("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadDouble("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadLong("Invalid"));
	        Assert.Throws<ArgumentException>(() => yxdb.ReadByte("Invalid"));
	        try
	        {
		        yxdb.ReadString(0);
	        }
	        catch (Exception ex)
	        {
		        _output.WriteLine(ex.ToString());
	        }
	        yxdb.Close();
        }

        [Fact]
        public void TestTutorialData()
        {
	        var path = GetPath("TutorialData.yxdb");
	        var yxdb = new YxdbReader(path);

	        long? mrCount = 0;
	        while (yxdb.Next())
	        {
		        if (yxdb.ReadString("Prefix") == "Mr")
		        {
			        mrCount++;
		        }
	        }
	        Assert.Equal(4068, mrCount);
	        yxdb.Close();
        }

        [Fact]
        public void TestNewYxdb()
        {
	        var path = GetPath("TestNewYxdb.yxdb");
	        var yxdb = new YxdbReader(path);
	        byte? sum = 0;
	        while (yxdb.Next())
	        {
		        sum += yxdb.ReadByte(1);
	        }
	        Assert.Equal((byte)6, sum);
	        yxdb.Close();
        }

        [Fact]
        public void TestVeryLongField()
        {
	        var path = GetPath("VeryLongField.yxdb");
	        var yxdb = new YxdbReader(path);

	        yxdb.Next();
	        var blob = yxdb.ReadBlob(1);
	        Assert.Equal(604732, blob.Length);

	        yxdb.Next();
	        blob = yxdb.ReadBlob(1);
	        Assert.Null(blob);

	        yxdb.Next();
	        blob = yxdb.ReadBlob(1);
	        Assert.Equal(604732, blob.Length);
	        
	        yxdb.Close();
        }

        [Fact]
        public void TestInvalidFile()
        {
	        try
	        {
		        var _ = new YxdbReader(GetPath("invalid.txt"));
		        Assert.True(false, "expected code to throw but it did not");
	        }
	        catch (Exception ex)
	        {
		        Assert.Equal("file is not a valid YXDB file", ex.Message);
		        _output.WriteLine(ex.ToString());
	        }
        }

        [Fact]
        public void TestSmallInvalidFile()
        {
	        try
	        {
		        var _ = new YxdbReader(GetPath("invalidSmall.txt"));
		        Assert.True(false, "expected code to throw but it did not");
	        }
	        catch (Exception ex)
	        {
		        Assert.Equal("file is not a valid YXDB file", ex.Message);
		        _output.WriteLine(ex.ToString());
	        }
        }

        private static string GetPath(string fileName)
        {
            return $"../../../test_files/{fileName}";
        }

        private const string AllNormalFieldsMetaXml = @"<RecordInfo>
	<Field name=""ByteField"" source=""TextInput:"" type=""Byte""/>
	<Field name=""BoolField"" source=""Formula: 1"" type=""Bool""/>
	<Field name=""Int16Field"" source=""Formula: 16"" type=""Int16""/>
	<Field name=""Int32Field"" source=""Formula: 32"" type=""Int32""/>
	<Field name=""Int64Field"" source=""Formula: 64"" type=""Int64""/>
	<Field name=""FixedDecimalField"" scale=""6"" size=""19"" source=""Formula: 123.45"" type=""FixedDecimal""/>
	<Field name=""FloatField"" source=""Formula: 678.9"" type=""Float""/>
	<Field name=""DoubleField"" source=""Formula: 0.12345"" type=""Double""/>
	<Field name=""StringField"" size=""64"" source=""Formula: &quot;A&quot;"" type=""String""/>
	<Field name=""WStringField"" size=""64"" source=""Formula: &quot;AB&quot;"" type=""WString""/>
	<Field name=""V_StringShortField"" size=""1000"" source=""Formula: &quot;ABC&quot;"" type=""V_String""/>
	<Field name=""V_StringLongField"" size=""2147483647"" source=""Formula: PadLeft(&quot;&quot;, 500, &apos;B&apos;)"" type=""V_String""/>
	<Field name=""V_WStringShortField"" size=""10"" source=""Formula: &quot;XZY&quot;"" type=""V_WString""/>
	<Field name=""V_WStringLongField"" size=""1073741823"" source=""Formula: PadLeft(&quot;&quot;, 500, &apos;W&apos;)"" type=""V_WString""/>
	<Field name=""DateField"" source=""Formula: &apos;2020-01-01&apos;"" type=""Date""/>
	<Field name=""DateTimeField"" source=""Formula: &apos;2020-02-03 04:05:06&apos;"" type=""DateTime""/>
</RecordInfo>
";
    }
}