using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command Wait() => WaitCommand.Command;

        private class WaitCommand : Command
        {
            internal static readonly Command Command = new WaitCommand();

            private WaitCommand() {}

            internal override IEnumerable<byte> Encode()
            {
                yield return 0b11111110;
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                // Nothing to do.
            }

            public override string ToString() => "Wait";
        }
    }
}
