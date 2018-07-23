using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    internal abstract class Decoder
    {
        internal abstract string Name { get; }
        internal abstract int Arity { get; }

        internal abstract bool CanDecode(int prefix);
        internal abstract Command Decode(int prefix, Func<int> nextByte);
        internal abstract Command DecodeText(IReadOnlyList<string> args);
    }
}
