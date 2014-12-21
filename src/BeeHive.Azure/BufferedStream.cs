using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BeeHive.Azure
{
    /// <summary>
    /// Not thread-safe, like any other stream
    /// </summary>
    internal class BufferedStream : Stream
    {
       
        private Func<long, byte[], int> _fillBuffer;
        private long _length;
        private long _currentLocalPosition = 0;
        private long _currentRemotePosition = 0;
        private List<byte> _buffer;
        private long _internalBufferSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillBuffer">delegate:
        /// int FillBuffer(long remoteOffset, byte[] buffer) 
        /// returns bytes read
        ///  </param>
        /// <param name="internalBufferSize"></param>
        public BufferedStream(long length, Func<long, byte[], int> fillBuffer, long internalBufferSize = 1024*1024 // 1MB
            )
        {
            _internalBufferSize = internalBufferSize;
            _length = length;
            _fillBuffer = fillBuffer;
        }

        public override void Flush()
        {
            _currentRemotePosition = 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientBuffer"></param>
        /// <param name="offset">position of the array</param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] clientBuffer, int offset, int count)
        {
            if (count + offset > clientBuffer.Length)
            {
                throw new ArgumentOutOfRangeException("count", "count + offset > clientBuffer.Length");
            }

            int countRead = 0;
            while (countRead < count)
            {
                if (_buffer == null || _buffer.Count == 0)
                {
                    if(!Fill())
                        break;
                }
                
                int countToRead = Math.Min(count - countRead, _buffer.Count);
                _buffer.CopyTo(0, clientBuffer, countRead + offset, countToRead);
                _currentLocalPosition += countToRead;
                countRead += countToRead;
                _buffer.RemoveRange(0, countToRead);
            }

            return countRead;
        }

        private bool Fill()
        {

            var bytes = new byte[_internalBufferSize];
            var read = _fillBuffer(_currentRemotePosition, bytes);
            if (read == 0)
                return false;

            _currentRemotePosition += read;
            _buffer = new List<byte>();
            if (read == _internalBufferSize)
            {
                _buffer.AddRange(bytes);                
            }
            else
            {
                var mainbuff = new byte[read];
                Array.Copy(bytes, 0, mainbuff, 0, read);
                _buffer.AddRange(mainbuff);                
            }

            return true;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _currentLocalPosition; }
            set
            {
                throw new NotSupportedException();                
            }
        }
    }
}
