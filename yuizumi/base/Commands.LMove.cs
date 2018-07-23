using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command LMove(Delta sld1, Delta sld2)
        {
            Requires.Arg(sld1.IsSld(), nameof(sld1), "Argument is not a valid sld.");
            Requires.Arg(sld2.IsSld(), nameof(sld2), "Argument is not a valid sld.");
            return new LMoveCommand(sld1, sld2);
        }

        private class LMoveCommand : Command
        {
            internal LMoveCommand(Delta sld1, Delta sld2)
            {
                mSld1 = sld1;
                mSld2 = sld2;
            }

            private readonly Delta mSld1;
            private readonly Delta mSld2;

            internal override IEnumerable<byte> Encode()
            {
                (int a1, int i1) = DeltaEncoder.EncodeSld(mSld1);
                (int a2, int i2) = DeltaEncoder.EncodeSld(mSld2);
                yield return (byte) (0b00001100 | (a1 << 4) | (a2 << 6));
                yield return (byte) (0b00000000 | (i1 << 0) | (i2 << 4));
            }

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mSld1;
                Coord c2 = c1 + mSld2;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
                Verify(state.Matrix.Contains(c2), $"{c2} is out of the matrix.");
                Region c0c1 = Region.Of(c0, c1);
                Verify(c0c1.GetMembers().All(c => state.Matrix[c] == Voxel.Void),
                       $"{c0c1} contains one or more Full coordinate.");
                Region c1c2 = Region.Of(c1, c2);
                Verify(c1c2.GetMembers().All(c => state.Matrix[c] == Voxel.Void),
                       $"{c1c2} contains one or more Full coordinate.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mSld1;
                Coord c2 = c1 + mSld2;
                foreach (Coord c in Region.Of(c0, c1).GetMembers())
                    yield return c;
                foreach (Coord c in Region.Of(c1, c2).GetMembers())
                    if (c != c1) yield return c;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                bot.Pos = bot.Pos + mSld1 + mSld2;
                state.Energy += 2 * (mSld1.Mlen() + 2 + mSld2.Mlen());
            }

            public override string ToString() => $"LMove {mSld1} {mSld2}";
        }

        private class LMoveDecoder : Decoder
        {
            internal override string Name => "LMove";

            internal override int Arity => 2;

            internal override bool CanDecode(int prefix)
                => (prefix & 0b00001111) == 0b00001100;

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                int suffix = nextByte();
                int a1 = (prefix & 0b00110000) >> 4;
                int i1 = (suffix & 0b00001111) >> 0;
                int a2 = (prefix & 0b11000000) >> 6;
                int i2 = (suffix & 0b11110000) >> 4;
                Delta sld1 = DeltaDecoder.DecodeSld(a1, i1);
                Delta sld2 = DeltaDecoder.DecodeSld(a2, i2);
                return Commands.LMove(sld1, sld2);
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.LMove(Delta.Parse(args[0]), Delta.Parse(args[1]));
        }
    }
}
