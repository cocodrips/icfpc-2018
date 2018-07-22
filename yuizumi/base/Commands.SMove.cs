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
    }
}
