using System.IO;

namespace Yuizumi.Icfpc2018
{
    internal static class StreamExtension
    {
        internal static int StrictReadByte(this Stream stream)
        {
            int value = stream.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return value;
        }
    }
}
