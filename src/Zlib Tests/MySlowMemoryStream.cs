using System;
using System.IO;

namespace Ionic.Zlib.Tests
{
    public class MySlowMemoryStream : MemoryStream
    {
        // ctor
        public MySlowMemoryStream(byte[] bytes) : base(bytes, false) { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            if (count == 0)
                return 0;

            // force stream to read just one byte at a time
            int NextByte = base.ReadByte();
            if (NextByte == -1)
                return 0;

            buffer[offset] = (byte)NextByte;
            return 1;
        }
    }



}
