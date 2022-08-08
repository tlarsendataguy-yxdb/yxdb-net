using System;
using System.IO;

namespace yxdb
{
    internal class BufferedRecordReader
    {
        private static readonly int LzfBufferSize = 262144;

        public BufferedRecordReader(Stream stream, int fixedLen, bool hasVarFields, long totalRecords)
        {
            _totalRecords = totalRecords;
            _stream = stream;
            _fixedLen = fixedLen;
            _hasVarFields = hasVarFields;
            RecordBuffer = hasVarFields ? new byte[fixedLen + 4 + 1000] : new byte[fixedLen];
            _lzfIn = new byte[LzfBufferSize];
            _lzfOut = new byte[LzfBufferSize];
            _lzf = new Lzf(_lzfIn, _lzfOut);
            _lzfLengthBuffer = new byte[4];
            _lzfOutIndex = 0;
            _lzfOutSize = 0;
        }
        
        private readonly Stream _stream;
        private readonly int _fixedLen;
        private readonly bool _hasVarFields;
        private readonly long _totalRecords;
        private readonly byte[] _lzfIn;
        private readonly byte[] _lzfOut;
        private int _lzfOutIndex;
        private int _lzfOutSize;
        private readonly Lzf _lzf;
        private readonly byte[] _lzfLengthBuffer;
        internal byte[] RecordBuffer;
        private int _recordBufferIndex;
        private long _currentRecord;

        public bool NextRecord()
        {
            _currentRecord++;
            if (_currentRecord > _totalRecords)
            {
                _stream.Dispose();
                return false;
            }

            _recordBufferIndex = 0;
            if (_hasVarFields)
            {
                ReadVariableRecord();
            }
            else
            {
                Read(_fixedLen);                
            }

            return true;
        }

        private void ReadVariableRecord()
        {
            Read(_fixedLen + 4);
            var varLength = BitConverter.ToInt32(RecordBuffer, _recordBufferIndex - 4);
            if (_fixedLen + 4 + varLength > RecordBuffer.Length)
            {
                var newLength = (_fixedLen + 4 + varLength) * 2;
                var newBuffer = new byte[newLength];
                Array.Copy(RecordBuffer, 0, newBuffer, 0, _fixedLen+4);
                RecordBuffer = newBuffer;
            }
            Read(varLength);
        }

        private void Read(int size)
        {
            while (size > 0)
            {
                if (_lzfOutSize == 0)
                {
                    _lzfOutSize = ReadNextLzfBlock();
                }

                while (size + _lzfOutIndex > _lzfOutSize)
                {
                    size -= CopyRemainingLzfOutToRecord();
                    _lzfOutSize = ReadNextLzfBlock();
                    _lzfOutIndex = 0;
                }

                var lenToCopy = Math.Min(_lzfOutSize, size);
                Array.Copy(_lzfOut, _lzfOutIndex, RecordBuffer, _recordBufferIndex, lenToCopy);
                _lzfOutIndex += lenToCopy;
                _recordBufferIndex += lenToCopy;
                size -= lenToCopy;
            }
        }

        private int CopyRemainingLzfOutToRecord()
        {
            var remainingLzf = _lzfOutSize - _lzfOutIndex;
            Array.Copy(_lzfOut, _lzfOutIndex, RecordBuffer, _recordBufferIndex, remainingLzf);
            _recordBufferIndex += remainingLzf;
            return remainingLzf;
        }

        private int ReadNextLzfBlock()
        {
            var lzfBlockLength = ReadLzfBlockLength();
            var checkBit = lzfBlockLength & 0x80000000;
            if (checkBit > 0)
            {
                lzfBlockLength &= 0x7fffffff;
                return _stream.Read(_lzfOut, 0, lzfBlockLength);
            }
            var readIn = _stream.Read(_lzfIn, 0, lzfBlockLength);
            return _lzf.Decompress(readIn);
        }

        private int ReadLzfBlockLength()
        {
            var read = _stream.Read(_lzfLengthBuffer, 0, 4);
            if (read < 4)
            {
                throw new IOException("yxdb file is not valid");
            }
            return BitConverter.ToInt32(_lzfLengthBuffer, 0);
        }
    }
}