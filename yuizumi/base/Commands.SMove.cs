using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command SMove(Delta lld)
        {
            Requires.Arg(lld.IsLld(), nameof(lld), "Argument is not a valid lld.");
            return new SMoveCommand(lld);
        }

        private class SMoveCommand : Command
        {
            internal SMoveCommand(Delta lld)
            {
                mLld = lld;
            }

            private readonly Delta mLld;

            internal override IEnumerable<byte> Encode()
            {
                (int a, int i) = DeltaEncoder.EncodeLld(mLld);
                yield return (byte) (0b00000100 | (a << 4));
                yield return (byte) (0b00000000 | (i << 0));
            }

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mLld;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
                Region c0c1 = Region.Of(c0, c1);
                Verify(c0c1.GetMembers().All(c => state.Matrix[c] == Voxel.Void),
                       $"{c0c1} contains one or more Full coordinate.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
                => Region.Of(bot.Pos, bot.Pos + mLld).GetMembers();

            internal override void ApplyToState(State state, Nanobot bot)
            {
                bot.Pos += mLld;
                state.Energy += 2 * mLld.Mlen();
            }

            public override string ToString() => $"SMove {mLld}";
        }

        private class SMoveDecoder : Decoder
        {
            internal override string Name => "SMove";

            internal override int Arity => 1;

            internal override bool CanDecode(int prefix)
                => (prefix & 0b11001111) == 0b00000100;

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                int suffix = nextByte();
                int a = (prefix & 0b00110000) >> 4;
                int i = (suffix & 0b00011111) >> 0;
                Delta lld = DeltaDecoder.DecodeLld(a, i);
                return Commands.SMove(lld);
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.SMove(Delta.Parse(args[0]));
        }
    }
}
