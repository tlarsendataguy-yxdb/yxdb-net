using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb
{
    public class YxdbReader
    {
        private YxdbReader(string path)
        {
            _stream = File.Open(path, FileMode.Open);
            _fields = new List<MetaInfoField>();
            LoadHeaderAndMetaInfo();
        }

        private YxdbReader(Stream stream)
        {
            _stream = stream;
            _fields = new List<MetaInfoField>();
            LoadHeaderAndMetaInfo();
        }

        public long NumRecords;
        private int _metaInfoSize;
        public string MetaInfoStr;
        private readonly List<MetaInfoField> _fields;
        private readonly Stream _stream;
        private YxdbRecord _record;
        private BufferedRecordReader _recordReader;

        public void Close()
        {
            _stream.Close();
        }

        public bool Next()
        {
            return _recordReader.NextRecord();
        }

        public byte? ReadByte(int index)
        {
            return _record.ExtractByteFrom(index, _recordReader.RecordBuffer);
        }

        public byte? ReadByte(string name)
        {
            return _record.ExtractByteFrom(name, _recordReader.RecordBuffer);
        }

        public bool? ReadBool(int index)
        {
            return _record.ExtractBoolFrom(index, _recordReader.RecordBuffer);
        }

        public bool? ReadBool(string name)
        {
            return _record.ExtractBoolFrom(name, _recordReader.RecordBuffer);
        }

        public long? ReadLong(int index)
        {
            return _record.ExtractLongFrom(index, _recordReader.RecordBuffer);
        }

        public long? ReadLong(string name)
        {
            return _record.ExtractLongFrom(name, _recordReader.RecordBuffer);
        }

        public double? ReadDouble(int index)
        {
            return _record.ExtractDoubleFrom(index, _recordReader.RecordBuffer);
        }

        public double? ReadDouble(string name)
        {
            return _record.ExtractDoubleFrom(name, _recordReader.RecordBuffer);
        }

        public string ReadString(int index)
        {
            return _record.ExtractStringFrom(index, _recordReader.RecordBuffer);
        }

        public string ReadString(string name)
        {
            return _record.ExtractStringFrom(name, _recordReader.RecordBuffer);
        }

        public DateTime? ReadDate(int index)
        {
            return _record.ExtractDateFrom(index, _recordReader.RecordBuffer);
        }

        public DateTime? ReadDate(string name)
        {
            return _record.ExtractDateFrom(name, _recordReader.RecordBuffer);
        }

        public byte[] ReadBlob(int index)
        {
            return _record.ExtractBlobFrom(index, _recordReader.RecordBuffer);
        }

        public byte[] ReadBlob(string name)
        {
            return _record.ExtractBlobFrom(name, _recordReader.RecordBuffer);
        }

        private void LoadHeaderAndMetaInfo()
        {
            var header = GetHeader();
            NumRecords = LittleEndian.ToInt64(header, 104);
            _metaInfoSize = LittleEndian.ToInt32(header, 80);
            LoadMetaInfo();
            _record = YxdbRecord.FromFieldList(_fields);
            _recordReader = new BufferedRecordReader(_stream, _record.FixedSize, _record.HasVar, NumRecords);
        }

        private byte[] GetHeader()
        {
            var headerBytes = new byte[512];
            var written = _stream.Read(headerBytes, 0, 512);
            if (written < 512)
            {
                CloseStreamAndThrow();
            }

            return headerBytes;
        }

        private void LoadMetaInfo()
        {
            var size = (_metaInfoSize * 2) - 2;
            var metaInfoBytes = new byte[size];
            var read = _stream.Read(metaInfoBytes, 0, size);
            if (read < size)
            {
                CloseStreamAndThrow();
            }

            var skipped = _stream.Seek(2, SeekOrigin.Current);
            if (skipped < 2)
            {
                CloseStreamAndThrow();
            }

            MetaInfoStr = System.Text.Encoding.Unicode.GetString(metaInfoBytes);
            GetFields();
        }

        private void GetFields()
        {
            var nodes = GetRecordInfoNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                ParseField(node);
            }
        }

        private XmlNodeList GetRecordInfoNodes()
        {
            var doc = new XmlDocument();
            doc.Load(new StringReader(MetaInfoStr));
            doc.Normalize();
            var info = doc.GetElementsByTagName("RecordInfo")[0];
            return info.ChildNodes;
        }

        private void ParseField(XmlNode field)
        {
            var attributes = field.Attributes;
            if (attributes == null)
            {
                CloseStreamAndThrow();
                return;
            }
            var name = attributes.GetNamedItem("name");
            var type = attributes.GetNamedItem("type");
            var size = attributes.GetNamedItem("size");
            var scale = attributes.GetNamedItem("scale");

            if (name == null || type == null)
            {
                CloseStreamAndThrow();
                return;
            }

            var nameStr = name.Value;
            switch (type.Value)
            {
                case "Byte":
                    _fields.Add(new MetaInfoField(nameStr, "Byte", 1, 0));
                    break;
                case "Bool":
                    _fields.Add(new MetaInfoField(nameStr, "Bool", 1, 0));
                    break;
                case "Int16":
                    _fields.Add(new MetaInfoField(nameStr, "Int16", 2, 0));
                    break;
                case "Int32":
                    _fields.Add(new MetaInfoField(nameStr, "Int32", 4, 0));
                    break;
                case "Int64":
                    _fields.Add(new MetaInfoField(nameStr, "Int64", 8, 0));
                    break;
                case "FixedDecimal":
                    if (scale == null || size == null)
                    {
                        CloseStreamAndThrow();
                        return;
                    }
                    _fields.Add(new MetaInfoField(nameStr, "FixedDecimal", int.Parse(size.Value), int.Parse(scale.Value)));
                    break;
                case "Float":
                    _fields.Add(new MetaInfoField(nameStr, "Float", 4, 0));
                    break;
                case "Double":
                    _fields.Add(new MetaInfoField(nameStr, "Double", 8, 0));
                    break;
                case "String":
                    if (size == null)
                    {
                        CloseStreamAndThrow();
                        return;
                    }
                    _fields.Add(new MetaInfoField(nameStr, "String", int.Parse(size.Value), 0));
                    break;
                case "WString":
                    if (size == null)
                    {
                        CloseStreamAndThrow();
                        return;
                    }
                    _fields.Add(new MetaInfoField(nameStr, "WString", int.Parse(size.Value), 0));
                    break;
                case "V_String":
                case "V_WString":
                case "Blob":
                case "SpatialObj":
                    _fields.Add(new MetaInfoField(nameStr, type.Value, 4, 0));
                    break;
                case "Date":
                    _fields.Add(new MetaInfoField(nameStr, "Date", 10, 0));
                    break;
                case "DateTime":
                    _fields.Add(new MetaInfoField(nameStr, "DateTime", 19, 0));
                    break;
                default:
                    CloseStreamAndThrow();
                    return;
            }
        }

        private void CloseStreamAndThrow()
        {
            _stream.Close();
            throw new ArgumentException("file is an invalid YXDB");
        }
    }
}