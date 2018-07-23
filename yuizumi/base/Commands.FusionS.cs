using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using Assignments = IEnumerable<(Nanobot, Command)>;

    public static partial class Commands
    {
        public static Command FusionS(Delta nd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            return new FusionSCommand(nd);
        }

        private class FusionSCommand : Command
        {
            internal FusionSCommand(Delta nd)
            {
                mNd = nd;
            }

            private readonly Delta mNd;

            internal override IEnumerable<byte> Encode()
            {
                int nd = DeltaEncoder.EncodeNd(mNd);
                yield return (byte) (0b00000110 | (nd << 3));
            }

            internal override void VerifyPreconds(State state, Nanobot botS)
            {
                Coord posS = botS.Pos;
                Coord posP = posS + mNd;
                Verify(state.Matrix.Contains(posP), $"{posP} is out of the matrix.");
            }

            internal override void VerifyPartners(Assignments assignments,
                                                  Nanobot botS)
            {
                (Nanobot botP, Command cmdP) = assignments.FirstOrDefault(
                    bot_cmd => bot_cmd.Item1.Pos == botS.Pos + mNd);
                Verify(botP != null, $"No bot exists at {botS.Pos + mNd}.");
                Verify(cmdP is FusionPCommand, $"{botP} is not performing FusionP.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot botS)
            {
                yield return botS.Pos;  // (botS.Pos + mNd) is cared by FusionP.
            }

            internal override void ApplyToState(State state, Nanobot botS)
            {
                // Everything is done by FusionP.
            }

            public override string ToString() => $"FusionS {mNd}";
        }

        private class FusionSDecoder : Decoder
        {
            internal override string Name => "FusionS";

            internal override int Arity => 1;

            internal override bool CanDecode(int prefix)
            {
                if (prefix == 0b11111110) return false;  // Wait
                return (prefix & 0b00000111) == 0b00000110;
            }

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                Delta nd = DeltaDecoder.DecodeNd((prefix & 0b11111000) >> 3);
                return Commands.FusionS(nd);
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.FusionS(Delta.Parse(args[0]));
        }
    }
}
