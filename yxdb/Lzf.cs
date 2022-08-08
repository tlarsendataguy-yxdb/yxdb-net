using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("yxdb_test")]
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
        private int _iidx;
        private int _oidx;
        private int _inLen;

        public int Decompress(int len)
        {
            _inLen = len;
            Reset();

            if (_inLen == 0)
            {
                return 0;
            }

            while (_iidx < _inLen)
            {
                var ctrl = _inBuffer[_iidx];
                _iidx++;

                if (ctrl < 32)
                {
                    CopyByteSequence(ctrl);
                }
                else
                {
                    ExpandRepeatedBytes(ctrl);
                }
            }

            return _oidx;
        }

        private void Reset()
        {
            _iidx = 0;
            _oidx = 0;
        }

        private void CopyByteSequence(byte ctrl)
        {
            var len = ctrl + 1;
            if (_oidx + len > _outBuffer.Length)
            {
                throw new ArgumentException("output array is too small");
            }
            Array.Copy(_inBuffer, _iidx, _outBuffer, _oidx, len);
            _oidx += len;
            _iidx += len;
        }

        private void ExpandRepeatedBytes(byte ctrl)
        {
            var length = ctrl >> 5;
            var reference = _oidx - ((ctrl & 0x1f) << 8) - 1;

            if (length == 7)
            {
                length += _inBuffer[_iidx];
                _iidx++;
            }

            if (_oidx + length + 2 > _outBuffer.Length)
            {
                throw new ArgumentException("output array is too small");
            }

            reference -= _inBuffer[_iidx];
            _iidx++;

            length += 2;

            while (length > 0)
            {
                var size = Math.Min(length, _oidx - reference);
                reference = CopyFromReferenceAndIncrement(reference, size);
                length -= size;
            }
        }

        private int CopyFromReferenceAndIncrement(int reference, int size)
        {
            Array.Copy(_outBuffer, reference, _outBuffer, _oidx, size);
            _oidx += size;
            return reference + size;
        }
    }
}