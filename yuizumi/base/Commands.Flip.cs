using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    using static Harmonics;

    public static partial class Commands
    {
        public static Command Flip() => FlipCommand.Command;

        private class FlipCommand : Command
        {
            internal static readonly Command Command = new FlipCommand();

            private FlipCommand() {}

            internal override IEnumerable<byte> Encode()
            {
                yield return 0b11111101;
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                state.Harmonics = (state.Harmonics == High) ? Low : High;
            }

            public override string ToString() => "Flip";
        }
    }
}
