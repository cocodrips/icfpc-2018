using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using static Harmonics;

    internal class Planer : Destroyer
    {
        internal Planer(State state) : base(state) {}

        private List<Harmonics> mHarmonics;

        internal override void Solve()
        {
            Inspect();

            MoveToY(MaxY + 1);
            Prepare();
            Execute();
            Cleanup();
            S.DoTurn(new [] {Commands.Halt()});
        }

        private void Inspect()
        {
            mHarmonics = new List<Harmonics>() { Low };

            var clusters = new CoordClusters();

            for (int y = MinY; y <= MaxY; y++) {
                for (int x = MinX; x <= MaxX; x++)
                for (int z = MinZ; z <= MaxZ; z++) {
                    if (S.Matrix[x, y, z] == Voxel.Void)
                        continue;
                    Coord c = Coord.Of(x, y, z);
                    clusters.Add(c);
                    if (x > MinX && S.Matrix[x - 1, y, z] == Voxel.Full)
                        clusters.Unite(c, Coord.Of(x - 1, y, z));
                    if (y > MinY && S.Matrix[x, y - 1, z] == Voxel.Full)
                        clusters.Unite(c, Coord.Of(x, y - 1, z));
                    if (z > MinZ && S.Matrix[x, y, z - 1] == Voxel.Full)
                        clusters.Unite(c, Coord.Of(x, y, z - 1));
                }
                mHarmonics.Add(clusters.Count == 1 ? Low : High);
            }
        }

        private void Prepare()
        {
            var coords = new List<int>();

            for (int i = R - 1; i > 0; i -= 31) {
                coords.Add(i);
                coords.Add(Math.Max(i - 30, 0));
            }

            int n = coords.Count;

            var upper = new List<Coord>();
            for (int i = 0; i < n; i++)
            for (int j = 0; j < i; j++) {
                upper.Add(Coord.Of(coords[i], MaxY + 1, coords[j]));
            }
            upper.Sort((c1, c2) => {
                int n1 = c1.X + c1.Z;
                int n2 = c2.X + c2.Z;
                return -Comparer<Int32>.Default.Compare(n1, n2);
            });

            var dests = new List<Coord>();
            dests.Add(Coord.Of(0, MaxY + 1, 0));

            for (int i = 0; i <= n - 2; i++) {
                dests.Add(Coord.Of(coords[i], MaxY + 1, coords[i]));
            }
            foreach (Coord c in upper) {
                dests.Add(c); dests.Add(Coord.Of(c.Z, c.Y, c.X));
            }

            LocateBots(dests);
        }

        private void Execute()
        {
            Delta down = Delta.LinearY(-1);

            var gvoid = new Command[S.Bots.Count];
            for (int i = 0; i < S.Bots.Count; i++) {
                int dx = (S.Bots[i].Pos.X % 31 == (R - 1) % 31) ? -30 : +30;
                if (S.Bots[i].Pos.X == 0) dx = (R + 30) % 31;
                dx = Math.Max(dx, -S.Bots[i].Pos.X);
                int dz = (S.Bots[i].Pos.Z % 31 == (R - 1) % 31) ? -30 : +30;
                if (S.Bots[i].Pos.Z == 0) dz = (R + 30) % 31;
                dz = Math.Max(dz, -S.Bots[i].Pos.Z);
                gvoid[i] = Commands.GVoid(down, Delta.Of(dx, 0, dz));
            }

            var smove = new Command[S.Bots.Count];
            for (int i = 0; i < S.Bots.Count; i++)
                smove[i] = Commands.SMove(down);

            var flipw = new Command[S.Bots.Count];
            for (int i = 0; i < S.Bots.Count; i++)
                flipw[i] = (i == 0) ? Commands.Flip() : Commands.Wait();

            while (S.Bots[0].Pos.Y > 0) {
                if (S.Harmonics == Low && mHarmonics[S.Bots[0].Pos.Y - 1] == High)
                    S.DoTurn(flipw);
                S.DoTurn(gvoid);
                if (S.Harmonics == High && mHarmonics[S.Bots[0].Pos.Y - 1] == Low)
                    S.DoTurn(flipw);
                S.DoTurn(smove);
            }
        }

        private void Cleanup()
        {
            var commands = new List<Command>();

            foreach (Nanobot bot in S.Bots) {
                if (bot.Pos.X < R - 1) {
                    if (bot.Pos.X % 31 == (R - 1) % 31) {
                        commands.Add(Commands.FusionP(Delta.LinearX(+1)));
                        continue;
                    }
                    if (bot.Pos.X % 31 == (R - 0) % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearX(-1)));
                        continue;
                    }
                }
                commands.Add(Commands.Wait());
            }
            S.DoTurn(commands);
            commands.Clear();

            foreach (Nanobot bot in S.Bots) {
                if (bot.Pos.Z < R - 1) {
                    if (bot.Pos.Z % 31 == (R - 1) % 31) {
                        commands.Add(Commands.FusionP(Delta.LinearZ(+1)));
                        continue;
                    }
                    if (bot.Pos.Z % 31 == (R - 0) % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearZ(-1)));
                        continue;
                    }
                }
                commands.Add(Commands.Wait());
            }
            S.DoTurn(commands);
            commands.Clear();

            while (true) {
                int x = 0;
                foreach (Nanobot bot in S.Bots)
                    x = Math.Max(bot.Pos.X, x);
                if (x == 0)
                    break;
                foreach (Nanobot bot in S.Bots) {
                    if (bot.Pos.X <= x - 2) {
                        commands.Add(Commands.Wait());
                        continue;
                    }
                    if (bot.Pos.X == x - 1) {
                        commands.Add(Commands.FusionP(Delta.LinearX(+1)));
                        continue;
                    }
                    if (x == 1 || x % 31 == R % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearX(-1)));
                    } else {
                        int dx = Math.Min(x - 1, 15);
                        commands.Add(Commands.SMove(Delta.LinearX(-dx)));
                    }
                }
                S.DoTurn(commands);
                commands.Clear();
            }

            while (true) {
                int z = 0;
                foreach (Nanobot bot in S.Bots)
                    z = Math.Max(bot.Pos.Z, z);
                if (z == 0)
                    break;
                foreach (Nanobot bot in S.Bots) {
                    if (bot.Pos.Z <= z - 2) {
                        commands.Add(Commands.Wait());
                        continue;
                    }
                    if (bot.Pos.Z == z - 1) {
                        commands.Add(Commands.FusionP(Delta.LinearZ(+1)));
                        continue;
                    }
                    if (z == 1 || z % 31 == R % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearZ(-1)));
                    } else {
                        int dz = -Math.Min(z - 1, 15);
                        commands.Add(Commands.SMove(Delta.LinearZ(dz)));
                    }
                }
                S.DoTurn(commands);
                commands.Clear();
            }
        }
    }
}
