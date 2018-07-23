using System;
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

        private class WaitDecoder : Decoder
        {
            internal override string Name => "Wait";

            internal override int Arity => 0;

            internal override bool CanDecode(int prefix)
                => prefix == 0b11111110;

            internal override Command Decode(int prefix, Func<int> nextByte)
                => Commands.Wait();

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.Wait();
        }
    }
}
