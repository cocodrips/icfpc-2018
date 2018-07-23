using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command Fill(Delta nd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            return new FillCommand(nd);
        }

        private class FillCommand : Command
        {
            internal FillCommand(Delta nd)
            {
                mNd = nd;
            }

            private readonly Delta mNd;

            internal override IEnumerable<byte> Encode()
            {
                int nd = DeltaEncoder.EncodeNd(mNd);
                yield return (byte) (0b00000011 | (nd << 3));
            }

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mNd;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
                yield return bot.Pos + mNd;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                state.FillVoxel(bot.Pos + mNd);
            }

            public override string ToString() => $"Fill {mNd}";
        }

        private class FillDecoder : Decoder
        {
            internal override string Name => "Fill";

            internal override int Arity => 1;

            internal override bool CanDecode(int prefix)
                => (prefix & 0b00000111) == 0b00000011;

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                Delta nd = DeltaDecoder.DecodeNd((prefix & 0b11111000) >> 3);
                return Commands.Fill(nd);
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.Fill(Delta.Parse(args[0]));
        }
    }
}
