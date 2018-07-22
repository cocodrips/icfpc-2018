using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command Halt() => HaltCommand.Command;

        private class HaltCommand : Command
        {
            internal static readonly Command Command = new HaltCommand();

            private HaltCommand() {}

            internal override IEnumerable<byte> Encode()
            {
                yield return 0b11111111;
            }

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Verify(bot.Pos == Coord.Zero, "bot.Pos");
                Verify(state.Bots.Count == 1 && state.Bots[0] == bot, "state.Bots");
                Verify(state.Harmonics == Harmonics.Low, "state.Harmonics");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                state.RemoveBot(bot);
            }

            public override string ToString() => "Halt";
        }
    }
}
