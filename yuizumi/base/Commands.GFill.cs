using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using Assignments = IEnumerable<(Nanobot, Command)>;

    public static partial class Commands
    {
        public static Command GFill(Delta nd, Delta fd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            Requires.Arg(fd.IsFd(), nameof(fd), "Argument is not a valid fd.");
            return new GFillCommand(nd, fd);
        }

        private class GFillCommand : Command
        {
            internal GFillCommand(Delta nd, Delta fd)
            {
                mNd = nd;
                mFd = fd;
            }

            private readonly Delta mNd;
            private readonly Delta mFd;

            private bool IsOrigin
                => (mFd.DX >= 0) && (mFd.DY >= 0) && (mFd.DZ >= 0);

            internal override IEnumerable<byte> Encode()
            {
                int nd = DeltaEncoder.EncodeNd(mNd);
                (int fx, int fy, int fz) = DeltaEncoder.EncodeFd(mFd);

                yield return (byte) (0b00000001 | (nd << 3));
                yield return (byte) (0b00000000 | (fx << 0));
                yield return (byte) (0b00000000 | (fy << 0));
                yield return (byte) (0b00000000 | (fz << 0));
            }

            private Region GetRegion(Nanobot bot)
                => Region.Of(bot.Pos + mNd, bot.Pos + mNd + mFd);

            internal override void VerifyPreconds(State state, Nanobot bot)
            {
                Coord c0 = bot.Pos;
                Coord c1 = c0 + mNd;
                Coord c2 = c1 + mFd;
                Verify(state.Matrix.Contains(c1), $"{c1} is out of the matrix.");
                Verify(state.Matrix.Contains(c2), $"{c2} is out of the matrix.");
            }

            internal override void VerifyPartners(Assignments assignments,
                                                  Nanobot thisBot)
            {
                Region r = this.GetRegion(thisBot);
                int n = 1;

                foreach ((Nanobot thatBot, Command cmd) in assignments) {
                    if (thisBot == thatBot) continue;

                    if (cmd is GFillCommand that && that.GetRegion(thatBot).Equals(r)) {
                        Coord thisPos = thisBot.Pos + this.mNd;
                        Coord thatPos = thatBot.Pos + that.mNd;
                        Verify(thisPos != thatPos,
                               $"{thisBot} and {thatBot} are both pointing to {thisPos}.");
                        ++n;
                    }
                }

                Verify(n == (1 << r.Dim()), "Incomplete GFill.");
            }

            internal override IEnumerable<Coord> GetVolatile(Nanobot bot)
            {
                yield return bot.Pos;

                if (!IsOrigin) yield break;

                Region r = Region.Of(bot.Pos + mNd, bot.Pos + mNd + mFd);
                foreach (Coord c in r.GetMembers())
                    yield return c;
            }

            internal override void ApplyToState(State state, Nanobot bot)
            {
                if (!IsOrigin) return;

                Region r = Region.Of(bot.Pos + mNd, bot.Pos + mNd + mFd);
                foreach (Coord c in r.GetMembers())
                    state.FillVoxel(c);
            }

            public override string ToString() => $"GFill {mNd} {mFd}";
        }
    }
}
