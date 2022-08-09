using System;
using System.Collections.Generic;
using Xunit;
using yxdb;

namespace yxdb_test
{
    public class YxdbRecordTest
    {
        [Fact]
        public void TestReadInt16Record()
        {
            var record = LoadRecordWithValueColumn("Int16", 2);
            var source = new byte[] { 23, 0, 0 };
            
            Assert.Equal(1, record.Fields.Count);
            Assert.Equal("value", record.Fields[0].Name);
            Assert.Equal(YxdbField.DataType.Long, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(3, record.FixedSize);
            Assert.Equal(23, record.ExtractLongFrom(0, source));
            Assert.Equal(23, record.ExtractLongFrom("value", source));
        }

        [Fact]
        public void TestReadInt32Record()
        {
            var record = LoadRecordWithValueColumn("Int32", 4);
            var source = new byte[] { 23, 0, 0, 0, 0};
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(5, record.FixedSize);
            Assert.Equal(23, record.ExtractLongFrom(0, source));
        }

        [Fact]
        public void TestReadInt64Record()
        {
            var record = LoadRecordWithValueColumn("Int64", 8);
            var source = new byte[] { 23, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(9, record.FixedSize);
            Assert.Equal(23, record.ExtractLongFrom(0, source));
        }

        [Fact]
        public void TestReadFloatRecord()
        {
            var record = LoadRecordWithValueColumn("Float", 4);
            var source = new byte[] { 205, 206, 140, 63, 0 };
            var expected = BitConverter.ToSingle(new byte[] { 205, 206, 140, 63 }); 
            
            Assert.Equal(YxdbField.DataType.Double, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(5, record.FixedSize);
            Assert.Equal(expected, record.ExtractDoubleFrom(0, source));
            Assert.Equal(expected, record.ExtractDoubleFrom("value", source));
        }

        [Fact] public void TestReadDoubleRecord()
        {
            var record = LoadRecordWithValueColumn("Double", 8);
            var source = new byte[] { 154, 155, 155, 155, 155, 155, 241, 63, 0 };
            var expected = BitConverter.ToDouble(new byte[] { 154, 155, 155, 155, 155, 155, 241, 63 }); 
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(9, record.FixedSize);
            Assert.Equal(expected, record.ExtractDoubleFrom(0, source));
        }

        [Fact] public void TestReadFixedDecimalRecord()
        {
            var record = LoadRecordWithValueColumn("FixedDecimal", 10);
            var source = new byte[] { 49, 50, 51, 46, 52, 53, 0, 43, 67, 110, 0 };
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(11, record.FixedSize);
            Assert.Equal(123.45, record.ExtractDoubleFrom(0, source));
        }
        
        [Fact]
        public void TestReadStringRecord()
        {
            var record = LoadRecordWithValueColumn("String", 15);
            var source = new byte[] { 104, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 33, 0, 23, 77, 0 };
            
            Assert.Equal(YxdbField.DataType.String, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(16, record.FixedSize);
            Assert.Equal("hello world!", record.ExtractStringFrom(0, source));
            Assert.Equal("hello world!", record.ExtractStringFrom("value", source));
        }
        
        [Fact]
        public void TestReadWStringRecord()
        {
            var record = LoadRecordWithValueColumn("WString", 15);
            var source = new byte[] { 104, 0, 101, 0, 108, 0, 108, 0, 111, 0, 32, 0, 119, 0, 111, 0, 114, 0, 108, 0, 100, 0, 33, 0, 0, 0, 23, 0, 77, 0, 0 };
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(31, record.FixedSize);
            Assert.Equal("hello world!", record.ExtractStringFrom(0, source));
        }
        
        [Fact]
        public void TestReadV_StringRecord()
        {
            var record = LoadRecordWithValueColumn("V_String", 15);
            var source = new byte[] { 0, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            
            Assert.Equal(true, record.HasVar);
            Assert.Equal(4, record.FixedSize);
            Assert.Equal("", record.ExtractStringFrom(0, source));
        }
        
        [Fact]
        public void TestReadV_WStringRecord()
        {
            var record = LoadRecordWithValueColumn("V_WString", 15);
            var source = new byte[] { 0, 0, 0, 0, 4, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            
            Assert.Equal(true, record.HasVar);
            Assert.Equal(4, record.FixedSize);
            Assert.Equal("", record.ExtractStringFrom(0, source));
        }
                
        [Fact]
        public void TestReadDateRecord()
        {
            var record = LoadRecordWithValueColumn("Date", 10);
            var source = new byte[] { 50, 48, 50, 49, 45, 48, 49, 45, 48, 49, 0 };
            var expected = new DateTime(2021, 1, 1);
            
            Assert.Equal(YxdbField.DataType.Date, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(11, record.FixedSize);
            Assert.Equal(expected, record.ExtractDateFrom(0, source));
            Assert.Equal(expected, record.ExtractDateFrom("value", source));
        }
                
        [Fact]
        public void TestReadDateTimeRecord()
        {
            var record = LoadRecordWithValueColumn("DateTime", 19);
            var source = new byte[] { 50, 48, 50, 49, 45, 48, 49, 45, 48, 50, 32, 48, 51, 58, 48, 52, 58, 48, 53, 0 };
            var expected = new DateTime(2021, 1, 2,3,4,5);
            
            Assert.Equal(false, record.HasVar);
            Assert.Equal(20, record.FixedSize);
            Assert.Equal(expected, record.ExtractDateFrom(0, source));
        }
                
        [Fact]
        public void TestReadBoolRecord()
        {
            var record = LoadRecordWithValueColumn("Bool", 1);
            var source = new byte[] { 1 };
            
            Assert.Equal(YxdbField.DataType.Boolean, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(1, record.FixedSize);
            Assert.Equal(true, record.ExtractBoolFrom(0, source));
            Assert.Equal(true, record.ExtractBoolFrom("value", source));
        }
                
        [Fact]
        public void TestReadByteRecord()
        {
            var record = LoadRecordWithValueColumn("Byte", 1);
            var source = new byte[] { 23, 0 };
            
            Assert.Equal(YxdbField.DataType.Byte, record.Fields[0].Type);
            Assert.Equal(false, record.HasVar);
            Assert.Equal(2, record.FixedSize);
            Assert.Equal((byte)23, record.ExtractByteFrom(0, source));
            Assert.Equal((byte)23, record.ExtractByteFrom("value", source));
        }
                
        [Fact]
        public void TestReadBlobRecord()
        {
            var record = LoadRecordWithValueColumn("Blob", 100);
            var source = new byte[] { 0, 0, 0, 0, 4, 0, 0, 0, 1,2,3,4,5,6,7,8 };
            
            Assert.Equal(YxdbField.DataType.Blob, record.Fields[0].Type);
            Assert.Equal(true, record.HasVar);
            Assert.Equal(4, record.FixedSize);
            Assert.Equal(new byte[]{}, record.ExtractBlobFrom(0, source));
            Assert.Equal(new byte[]{}, record.ExtractBlobFrom("value", source));
        }
                
        [Fact]
        public void TestReadSpatialObjRecord()
        {
            var record = LoadRecordWithValueColumn("SpatialObj", 100);
            var source = new byte[] { 0, 0, 0, 0, 4, 0, 0, 0, 1,2,3,4,5,6,7,8 };
            
            Assert.Equal(true, record.HasVar);
            Assert.Equal(4, record.FixedSize);
            Assert.Equal(new byte[]{}, record.ExtractBlobFrom(0, source));
        }

        private static YxdbRecord LoadRecordWithValueColumn(string type, int size)
        {
            var fields = new List<MetaInfoField>
            {
                new MetaInfoField("value", type, size, 0)
            };
            return YxdbRecord.FromFieldList(fields);
        }
    }
}