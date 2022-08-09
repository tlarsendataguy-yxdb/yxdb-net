using System;

namespace yxdb
{
    internal class Lzf
    {
        public Lzf(byte[] inBuffer, byte[] outBuffer)
        {
            _inBuffer = inBuffer;
            _outBuffer = outBuffer;
        }

        private readonly byte[] _inBuffer;
        private readonly byte[] _outBuffer;
        private int _inIndex;
        private int _outIndex;
        private int _inLen;

        public int Decompress(int len)
        {
            _inLen = len;
            Reset();

            if (_inLen == 0)
            {
                return 0;
            }

            while (_inIndex < _inLen)
            {
                var ctrl = _inBuffer[_inIndex];
                _inIndex++;

                if (ctrl < 32)
                {
                    CopyByteSequence(ctrl);
                }
                else
                {
                    ExpandRepeatedBytes(ctrl);
                }
            }

            return _outIndex;
        }

        private void Reset()
        {
            _inIndex = 0;
            _outIndex = 0;
        }

        private void CopyByteSequence(byte ctrl)
        {
            var len = ctrl + 1;
            if (_outIndex + len > _outBuffer.Length)
            {
                throw new ArgumentException("output array is too small");
            }
            Array.Copy(_inBuffer, _inIndex, _outBuffer, _outIndex, len);
            _outIndex += len;
            _inIndex += len;
        }

        private void ExpandRepeatedBytes(byte ctrl)
        {
            var length = ctrl >> 5;
            var reference = _outIndex - ((ctrl & 0x1f) << 8) - 1;

            if (length == 7)
            {
                length += _inBuffer[_inIndex];
                _inIndex++;
            }

            if (_outIndex + length + 2 > _outBuffer.Length)
            {
                throw new ArgumentException("output array is too small");
            }

            reference -= _inBuffer[_inIndex];
            _inIndex++;

            length += 2;

            while (length > 0)
            {
                var size = Math.Min(length, _outIndex - reference);
                reference = CopyFromReferenceAndIncrement(reference, size);
                length -= size;
            }
        }

        private int CopyFromReferenceAndIncrement(int reference, int size)
        {
            Array.Copy(_outBuffer, reference, _outBuffer, _outIndex, size);
            _outIndex += size;
            return reference + size;
        }
    }
}