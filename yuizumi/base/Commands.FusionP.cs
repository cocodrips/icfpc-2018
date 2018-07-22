using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using Assignments = IEnumerable<(Nanobot, Command)>;

    public static partial class Commands
    {
        public static Command FusionP(Delta nd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            return new FusionPCommand(nd);
        }

        private class FusionPCommand : Command
        {
            internal FusionPCommand(Delta nd)
            {
                mNd = nd;
            }

            private readonly Delta mNd;

            internal override IEnumerable<byte> Encode()
            {
                int nd = DeltaEncoder.EncodeNd(mNd);
                yield return (byte) (0b00000111 | (nd << 3));
            }

            internal override void VerifyPreconds(State state, Nanobot botP)
            {
                Coord posP = botP.Pos;
                Coord posS = posP + mNd;
                Verify(state.Matrix.Contains(posS), $"{posS} is out of the matrix.");
            }

            internal override void VerifyPartners(Assignments assignments,
                                                  Nanobot botP)
            {
                (Nanobot botS, Command cmdS) = assignments.FirstOrDefault(
                    bot_cmd => bot_cmd.Item1.Pos == botP.Pos + mNd);
                Verify(botS != null, $"No bot exists at {botP.Pos + mNd}.");
                Verify(cmdS is FusionSCommand, $"{botS} is not performing FusionS.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot botP)
            {
                yield return botP.Pos;  // (botP.Pos + mNd) is cared by FusionS.
            }

            internal override void ApplyToState(State state, Nanobot botP)
            {
                Nanobot botS = state.Bots.First(bot => bot.Pos == botP.Pos + mNd);
                state.RemoveBot(botS);
                botP.Fusion(botS);

                state.Energy -= 24;
            }

            public override string ToString() => $"FusionP {mNd}";
        }
    }
}
