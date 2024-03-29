using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command Fission(Delta nd, int m)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            Requires.Range(m, nameof(m), 0, 255);
            return new FissionCommand(nd, m);
        }

        private class FissionCommand : Command
        {
            internal FissionCommand(Delta nd, int m)
            {
                mNd = nd;
                mM = m;
            }

            private readonly Delta mNd;
            private readonly int mM;

            internal override IEnumerable<byte> Encode()
            {
                int nd = DeltaEncoder.EncodeNd(mNd);
                yield return (byte) (0b00000101 | (nd << 3));
                yield return (byte) (0b00000000 | (mM << 0));
            }

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mNd;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
                Verify(state.Matrix[c1] == Voxel.Void, $"{c1} is Full.");
                Verify(bot.Seeds.Count >= mM + 1, $"The bot is lack of seeds.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
                yield return bot.Pos + mNd;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                state.InsertBot(bot.Fission(bot.Pos + mNd, mM));
                state.Energy += 24;
            }

            public override string ToString() => $"Fission {mNd} {mM}";
        }

        private class FissionDecoder : Decoder
        {
            internal override string Name => "Fission";

            internal override int Arity => 2;

            internal override bool CanDecode(int prefix)
            {
                if (prefix == 0b11111101) return false;  // Flip
                return (prefix & 0b00000111) == 0b00000101;
            }

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                Delta nd = DeltaDecoder.DecodeNd((prefix & 0b11111000) >> 3);
                return Commands.Fission(nd, nextByte());
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.Fission(Delta.Parse(args[0]), Int32.Parse(args[1]));
        }
    }
}
