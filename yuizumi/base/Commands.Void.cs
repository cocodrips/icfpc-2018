using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static partial class Commands
    {
        public static Command Void(Delta nd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            return new VoidCommand(nd);
        }

        private class VoidCommand : Command
        {
            internal VoidCommand(Delta nd)
            {
                mNd = nd;
            }

            private readonly Delta mNd;

            internal override IEnumerable<byte> Encode()
                => throw new System.NotImplementedException();

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mNd;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;
                yield return bot.Pos + mNd;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                state.VoidVoxel(bot.Pos + mNd);
            }

            public override string ToString() => $"Void {mNd}";
        }
    }
}
