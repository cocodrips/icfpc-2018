using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        internal static readonly IReadOnlyList<Decoder> Decoders = new Decoder[] {
            new HaltDecoder(),
            new WaitDecoder(),
            new FlipDecoder(),
            new SMoveDecoder(),
            new LMoveDecoder(),
            new FusionPDecoder(),
            new FusionSDecoder(),
            new FissionDecoder(),
            new FillDecoder(),
            new VoidDecoder(),
            new GFillDecoder(),
            new GVoidDecoder(),
        };
    }
}
