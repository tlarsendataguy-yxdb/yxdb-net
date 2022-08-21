using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

[assembly: InternalsVisibleTo("yxdb_test")]
namespace yxdb
{
    /// <summary>
    /// <para>
    /// <c>YxdbReader</c> contains the public interface for reading .yxdb files.
    /// </para>
    /// <para>
    /// There are 2 constructors available for <c>YxdbReader</c>. One constructor takes a file path string and
    /// another takes a <c>Stream</c> that reads yxdb-formatted bytes.
    /// </para>
    /// </summary>
    public class YxdbReader
    {
        private const string InvalidYxdbMsg = "file is not a valid YXDB file";

        /// <summary>
        /// Loads a .yxdb file from the file system.
        /// </summary>
        /// <param name="path">The path to the .yxdb file.</param>
        public YxdbReader(string path)
        {
            _stream = File.Open(path, FileMode.Open);
            _fields = new List<MetaInfoField>();
            LoadHeaderAndMetaInfo();
        }

        /// <summary>
        /// Reads a .yxdb file from a <c>Stream</c> of bytes.
        /// </summary>
        /// <param name="stream">The stream containing the .yxdb-formatted data</param>
        public YxdbReader(Stream stream)
        {
            _stream = stream;
            _fields = new List<MetaInfoField>();
            LoadHeaderAndMetaInfo();
        }

        /// <summary>
        /// The total number of records in the .yxdb file.
        /// </summary>
        public long NumRecords;
        private int _metaInfoSize;
        /// <summary>
        /// Contains the raw XML metadata from the .yxdb file.
        /// </summary>
        public string MetaInfoStr;
        private readonly List<MetaInfoField> _fields;
        private readonly Stream _stream;
        private YxdbRecord _record;
        private BufferedRecordReader _recordReader;

        /// <summary>
        /// Returns the list of fields in the .yxdb file. The index of each field in this list matches the index of
        /// the field in the .yxdb file.
        /// </summary>
        public List<YxdbField> ListFields()
        {
            return _record.Fields;
        }
        
        /// <summary>
        /// Closes the stream manually if the reader needs to be ended before reaching the end of the file. Also
        /// disposes the underlying stream.
        /// </summary>
        public void Close()
        {
            _recordReader.Close();
        }

        /// <summary>
        /// <para>
        /// The <c>Next</c> function is designed to iterate over each record in the .yxdb file.
        /// </para>
        /// <para>
        /// The standard way of iterating through records is to use a while loop:
        /// </para>
        /// <code>
        /// while (reader.Next()) {
        ///     // do something
        /// }
        /// </code>
        /// </summary>
        public bool Next()
        {
            return _recordReader.NextRecord();
        }

        /// <summary>
        /// Reads a byte field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the byte field at the specified index. May be null.</returns>
        public byte? ReadByte(int index)
        {
            return _record.ExtractByteFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a byte field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified byte field. May be null.</returns>
        public byte? ReadByte(string name)
        {
            return _record.ExtractByteFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a bool field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the bool field at the specified index. May be null.</returns>
        public bool? ReadBool(int index)
        {
            return _record.ExtractBoolFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a bool field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified bool field. May be null.</returns>
        public bool? ReadBool(string name)
        {
            return _record.ExtractBoolFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a long integer field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the long integer field at the specified index. May be null.</returns>
        public long? ReadLong(int index)
        {
            return _record.ExtractLongFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a long integer field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified long integer field. May be null.</returns>
        public long? ReadLong(string name)
        {
            return _record.ExtractLongFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a numeric field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the long integer field at the specified index. May be null.</returns>
        public double? ReadDouble(int index)
        {
            return _record.ExtractDoubleFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// reads a numeric field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified numeric field. May be null.</returns>
        public double? ReadDouble(string name)
        {
            return _record.ExtractDoubleFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a text field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0</param>
        /// <returns>The value of the text field at the specified index. May be null.</returns>
        public string ReadString(int index)
        {
            return _record.ExtractStringFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a text field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified text field. May be null.</returns>
        public string ReadString(string name)
        {
            return _record.ExtractStringFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a date/datetime field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the date/datetime field at the specified index. May be null.</returns>
        public DateTime? ReadDate(int index)
        {
            return _record.ExtractDateFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a date/datetime field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified date/datetime field. May be null.</returns>
        public DateTime? ReadDate(string name)
        {
            return _record.ExtractDateFrom(name, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a blob field from the .yxdb file.
        /// </summary>
        /// <param name="index">The index of the field to read, starting at 0.</param>
        /// <returns>The value of the blob field at the specified index. May be null.</returns>
        public byte[] ReadBlob(int index)
        {
            return _record.ExtractBlobFrom(index, _recordReader.RecordBuffer);
        }

        /// <summary>
        /// Reads a blob field from the .yxdb file.
        /// </summary>
        /// <param name="name">The name of the field to read.</param>
        /// <returns>The value of the specified blob field. May be null.</returns>
        public byte[] ReadBlob(string name)
        {
            return _record.ExtractBlobFrom(name, _recordReader.RecordBuffer);
        }

        private void LoadHeaderAndMetaInfo()
        {
            try
            {
                var header = GetHeader();
                var fileType = Encoding.UTF8.GetString(header, 0, 21);
                if (!"Alteryx Database File".Equals(fileType))
                {
                    CloseStreamAndThrow();
                }
                NumRecords = LittleEndian.ToInt64(header, 104);
                _metaInfoSize = LittleEndian.ToInt32(header, 80);
                LoadMetaInfo();
                _record = YxdbRecord.FromFieldList(_fields);
                _recordReader = new BufferedRecordReader(_stream, _record.FixedSize, _record.HasVar, NumRecords);
            }
            catch (Exception)
            {
                throw new Exception(InvalidYxdbMsg);
            }
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

            MetaInfoStr = Encoding.Unicode.GetString(metaInfoBytes);
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
            Close();
            throw new ArgumentException(InvalidYxdbMsg);
        }
    }
}