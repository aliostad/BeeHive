using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace BeeHive.Azure.Tests
{

    public class BufferedStreamTests
    {

        [Theory]
        [InlineData(1000, 7, 3)]
        [InlineData(1000, 3, 7)]
        [InlineData(1000, 9, 5)]
        [InlineData(1000, 15, 5)]
        [InlineData(1000, 5, 15)]
        [InlineData(1000, 7, 15)]
        [InlineData(1000, 15, 7)]
        public void TestBuffering(long length, int internalBufferCount, int clientBufferCount)
        {
            var random = new Random();
            var remoteData = new byte[length];
            random.NextBytes(remoteData);
            var buffer = new byte[clientBufferCount];
            var clientData = new List<byte>();
            Func<long, byte[], int> reader = (remoteOffset, buff) =>
            {
                int toRead = Math.Min(buff.Length, (int)(length - remoteOffset));
                Buffer.BlockCopy(remoteData, (int) remoteOffset, buff, 0, toRead);
                return  toRead;
            };
            var bufferedStream = new GenericBufferedStream(length, reader, internalBufferCount);
            while (true)
            {
                int read = bufferedStream.Read(buffer, 0, buffer.Length);
                clientData.AddRange(buffer.Take(read));
                if(read==0)
                    break;
                
            }

            Assert.Equal(remoteData, clientData);
        }
    }
}
