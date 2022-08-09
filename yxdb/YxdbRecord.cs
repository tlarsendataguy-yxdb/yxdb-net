using System;
using System.Collections.Generic;

namespace yxdb
{
    internal class YxdbRecord
    {
        private YxdbRecord(int fieldCount)
        {
            _nameToIndex = new Dictionary<string, int>(fieldCount);
            Fields = new List<YxdbField>(fieldCount);
            _boolExtractors = new Dictionary<int, Func<byte[], bool?>>();
            _byteExtractors = new Dictionary<int, Func<byte[], byte?>>();
            _longExtractors = new Dictionary<int, Func<byte[], long?>>();
            _doubleExtractors = new Dictionary<int, Func<byte[], double?>>();
            _stringExtractors = new Dictionary<int, Func<byte[], string>>();
            _dateExtractors = new Dictionary<int, Func<byte[], DateTime?>>();
            _blobExtractors = new Dictionary<int, Func<byte[], byte[]>>();
            FixedSize = 0;
            HasVar = false;
        }

        public readonly List<YxdbField> Fields;
        public int FixedSize;
        public bool HasVar;
        private readonly Dictionary<string, int> _nameToIndex;
        private readonly Dictionary<int, Func<byte[], bool?>> _boolExtractors;
        private readonly Dictionary<int, Func<byte[], byte?>> _byteExtractors;
        private readonly Dictionary<int, Func<byte[], long?>> _longExtractors;
        private readonly Dictionary<int, Func<byte[], double?>> _doubleExtractors;
        private readonly Dictionary<int, Func<byte[], string>> _stringExtractors;
        private readonly Dictionary<int, Func<byte[], DateTime?>> _dateExtractors;
        private readonly Dictionary<int, Func<byte[], byte[]>> _blobExtractors;

        public static YxdbRecord FromFieldList(List<MetaInfoField> fields)
        {
            var record = new YxdbRecord(fields.Count);
            var startAt = 0;
            foreach (var field in fields)
            {
                int size;
                switch (field.Type)
                {
                    case "Int16":
                        record.AddLongExtractor(field.Name, Extractors.NewInt16Extractor(startAt));
                        startAt += 3;
                        break;
                    case "Int32":
                        record.AddLongExtractor(field.Name, Extractors.NewInt32Extractor(startAt));
                        startAt += 5;
                        break;
                    case "Int64":
                        record.AddLongExtractor(field.Name, Extractors.NewInt64Extractor(startAt));
                        startAt += 9;
                        break;
                    case "Float":
                        record.AddDoubleExtractor(field.Name, Extractors.NewFloatExtractor(startAt));
                        startAt += 5;
                        break;
                    case "Double":
                        record.AddDoubleExtractor(field.Name, Extractors.NewDoubleExtractor(startAt));
                        startAt += 9;
                        break;
                    case "FixedDecimal":
                        size = field.Size;
                        record.AddDoubleExtractor(field.Name, Extractors.NewFixedDecimalExtractor(startAt, size));
                        startAt += size + 1;
                        break;
                    case "String":
                        size = field.Size;
                        record.AddStringExtractor(field.Name, Extractors.NewStringExtractor(startAt, size));
                        startAt += size + 1;
                        break;
                    case "WString":
                        size = field.Size;
                        record.AddStringExtractor(field.Name, Extractors.NewWStringExtractor(startAt, size));
                        startAt += (size * 2) + 1;
                        break;
                    case "V_String":
                        record.AddStringExtractor(field.Name, Extractors.NewV_StringExtractor(startAt));
                        startAt += 4;
                        record.HasVar = true;
                        break;
                    case "V_WString":
                        record.AddStringExtractor(field.Name, Extractors.NewV_WStringExtractor(startAt));
                        startAt += 4;
                        record.HasVar = true;
                        break;
                    case "Date":
                        record.AddDateExtractor(field.Name, Extractors.NewDateExtractor(startAt));
                        startAt += 11;
                        break;
                    case "DateTime":
                        record.AddDateExtractor(field.Name, Extractors.NewDateTimeExtractor(startAt));
                        startAt += 20;
                        break;
                    case "Bool":
                        record.AddBoolExtractor(field.Name, Extractors.NewBoolExtractor(startAt));
                        startAt += 1;
                        break;
                    case "Byte":
                        record.AddByteExtractor(field.Name, Extractors.NewByteExtractor(startAt));
                        startAt += 2;
                        break;
                    case "Blob":
                    case "SpatialObj":
                        record.AddBlobExtractor(field.Name, Extractors.NewBlobExtractor(startAt));
                        startAt += 4;
                        record.HasVar = true;
                        break;
                    default:
                        throw new ArgumentException("field type is not supported; yxdb metadata is not valid");
                }
            }

            record.FixedSize = startAt;
            return record;
        }

        public long? ExtractLongFrom(int index, byte[] buffer)
        {
            var extractor = _longExtractors[index];
            return extractor(buffer);
        }

        public long? ExtractLongFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractLongFrom(index, buffer);
        }

        public double? ExtractDoubleFrom(int index, byte[] buffer)
        {
            var extractor = _doubleExtractors[index];
            return extractor(buffer);
        }

        public double? ExtractDoubleFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractDoubleFrom(index, buffer);
        }

        public string ExtractStringFrom(int index, byte[] buffer)
        {
            var extractor = _stringExtractors[index];
            return extractor(buffer);
        }

        public string ExtractStringFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractStringFrom(index, buffer);
        }

        public DateTime? ExtractDateFrom(int index, byte[] buffer)
        {
            var extractor = _dateExtractors[index];
            return extractor(buffer);
        }

        public DateTime? ExtractDateFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractDateFrom(index, buffer);
        }

        public bool? ExtractBoolFrom(int index, byte[] buffer)
        {
            var extractor = _boolExtractors[index];
            return extractor(buffer);
        }

        public bool? ExtractBoolFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractBoolFrom(index, buffer);
        }

        public byte? ExtractByteFrom(int index, byte[] buffer)
        {
            var extractor = _byteExtractors[index];
            return extractor(buffer);
        }

        public byte? ExtractByteFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractByteFrom(index, buffer);
        }

        public byte[] ExtractBlobFrom(int index, byte[] buffer)
        {
            var extractor = _blobExtractors[index];
            return extractor(buffer);
        }

        public byte[] ExtractBlobFrom(string name, byte[] buffer)
        {
            var index = _nameToIndex[name];
            return ExtractBlobFrom(index, buffer);
        }

        private void AddLongExtractor(string name, Func<byte[], long?> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Long);
            _longExtractors[index] = extractor;
        }

        private void AddDoubleExtractor(string name, Func<byte[], double?> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Double);
            _doubleExtractors[index] = extractor;
        }

        private void AddStringExtractor(string name, Func<byte[], string> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.String);
            _stringExtractors[index] = extractor;
        }
        
        private void AddDateExtractor(string name, Func<byte[], DateTime?> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Date);
            _dateExtractors[index] = extractor;
        }

        private void AddBoolExtractor(string name, Func<byte[], bool?> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Boolean);
            _boolExtractors[index] = extractor;
        }

        private void AddByteExtractor(string name, Func<byte[], byte?> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Byte);
            _byteExtractors[index] = extractor;
        }

        private void AddBlobExtractor(string name, Func<byte[], byte[]> extractor)
        {
            var index = AddFieldNameToIndexMap(name, YxdbField.DataType.Blob);
            _blobExtractors[index] = extractor;
        }

        private int AddFieldNameToIndexMap(string name, YxdbField.DataType type)
        {
            var index = Fields.Count;
            Fields.Add(new YxdbField(name, type));
            _nameToIndex[name] = index;
            return index;
        }
    }
}