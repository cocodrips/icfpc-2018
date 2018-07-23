using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using Assignments = IEnumerable<(Nanobot, Command)>;

    public static partial class Commands
    {
        public static Command GVoid(Delta nd, Delta fd)
        {
            Requires.Arg(nd.IsNd(), nameof(nd), "Argument is not a valid nd.");
            Requires.Arg(fd.IsFd(), nameof(fd), "Argument is not a valid fd.");
            return new GVoidCommand(nd, fd);
        }

        private class GVoidCommand : Command
        {
            internal GVoidCommand(Delta nd, Delta fd)
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

                yield return (byte) (0b00000000 | (nd << 3));
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

                    if (cmd is GVoidCommand that && that.GetRegion(thatBot).Equals(r)) {
                        Coord thisPos = thisBot.Pos + this.mNd;
                        Coord thatPos = thatBot.Pos + that.mNd;
                        Verify(thisPos != thatPos,
                               $"{thisBot} and {thatBot} are both pointing to {thisPos}.");
                        ++n;
                    }
                }

                Verify(n == (1 << r.Dim()), "Incomplete GVoid.");
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
                    state.VoidVoxel(c);
            }

            public override string ToString() => $"GVoid {mNd} {mFd}";
        }

        private class GVoidDecoder : Decoder
        {
            internal override string Name => "GVoid";

            internal override int Arity => 2;

            internal override bool CanDecode(int prefix)
                => (prefix & 0b00000111) == 0b00000000;

            internal override Command Decode(int prefix, Func<int> nextByte)
            {
                Delta nd = DeltaDecoder.DecodeNd((prefix & 0b11111000) >> 3);
                Delta fd = DeltaDecoder.DecodeFd(nextByte(), nextByte(), nextByte());
                return Commands.GVoid(nd, fd);
            }

            internal override Command DecodeText(IReadOnlyList<string> args)
                => Commands.GVoid(Delta.Parse(args[0]), Delta.Parse(args[1]));
        }
    }
}
