using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    internal static class DestroyV1
    {
        internal static void Doit(State s)
        {
            s.DoesAutoVerify = false;
            LocateBots(s);
            VoidAll(s);
            GatherBots(s);
            s.DoTurn(new [] {Commands.Halt()});
        }

        private static void LocateBots(State s)
        {
            var commands = new List<Command>();
            int r = s.Matrix.R;

            while (s.Bots[0].Pos.Y < r - 1) {
                int dy = r - 1 - s.Bots[0].Pos.Y;
                s.DoTurn(new [] {Commands.SMove(Delta.LinearY(Math.Min(dy, 15)))});
            }

            var dests = new List<Coord>();
            {
                int[] coords = {r-1, r-31, r-32, r-62, r-63, 0};

                var upper = new List<Coord>();

                for (int i = 0; i < 6; i++)
                for (int j = 0; j < i; j++) {
                    upper.Add(Coord.Of(coords[i], r - 1, coords[j]));
                }
                upper.Sort((c1, c2) => {
                    int n1 = c1.X + c1.Z;
                    int n2 = c2.X + c2.Z;
                    return -Comparer<Int32>.Default.Compare(n1, n2);
                });

                dests.Add(Coord.Of(0, r - 1, 0));

                for (int i = 0; i <= 4; i++) {
                    dests.Add(Coord.Of(coords[i], r - 1, coords[i]));
                }
                foreach (Coord c in upper) {
                    dests.Add(c); dests.Add(Coord.Of(c.Z, c.Y, c.X));
                }
            }

            while (true) {
                commands.Clear();

                if (s.Bots.Count < dests.Count) {
                    int dx = (s.Bots.Count % 2 == 1) ? 1 : 0;
                    int dz = (s.Bots.Count % 2 == 1) ? 0 : 1;
                    commands.Add(Commands.Fission(Delta.Of(dx, 0, dz), 0));
                } else {
                    commands.Add(Commands.Wait());
                }

                for (int i = 1; i < s.Bots.Count; i++) {
                    int dx = dests[i].X - s.Bots[i].Pos.X;
                    int dz = dests[i].Z - s.Bots[i].Pos.Z;
                    if (dx > 0 && (i % 2 == 1 || dz == 0)) {
                        commands.Add(Commands.SMove(Delta.LinearX(Math.Min(dx, 15))));
                        continue;
                    }
                    if (dz > 0 && (i % 2 == 0 || dx == 0)) {
                        commands.Add(Commands.SMove(Delta.LinearZ(Math.Min(dz, 15))));
                        continue;
                    }
                    commands.Add(Commands.Wait());
                }

                if (IsNoop(commands)) break;
                s.DoTurn(commands);
            }
        }

        private static void VoidAll(State s)
        {
            var commands = new Command[s.Bots.Count];
            int r = s.Matrix.R;

            Delta down = Delta.LinearY(-1);

            while (s.Bots[0].Pos.Y > 0) {
                for (int i = 0; i < s.Bots.Count; i++) {
                    int dx = (s.Bots[i].Pos.X % 31 == (r - 1) % 31) ? -30 : +30;
                    if (s.Bots[i].Pos.X == 0) dx = r - 63;
                    dx = Math.Max(dx, -s.Bots[i].Pos.X);
                    int dz = (s.Bots[i].Pos.Z % 31 == (r - 1) % 31) ? -30 : +30;
                    if (s.Bots[i].Pos.Z == 0) dz = r - 63;
                    dz = Math.Max(dz, -s.Bots[i].Pos.Z);
                    commands[i] = Commands.GVoid(down, Delta.Of(dx, 0, dz));
                }
                s.DoTurn(commands);
                for (int i = 0; i < s.Bots.Count; i++) {
                    commands[i] = Commands.SMove(down);
                }
                s.DoTurn(commands);
            }
        }

        private static void GatherBots(State s)
        {
            var commands = new List<Command>();
            int r = s.Matrix.R;

            foreach (Nanobot bot in s.Bots) {
                if (bot.Pos.X < r - 1) {
                    if (bot.Pos.X % 31 == (r - 1) % 31) {
                        commands.Add(Commands.FusionP(Delta.LinearX(+1)));
                        continue;
                    }
                    if (bot.Pos.X % 31 == (r - 0) % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearX(-1)));
                        continue;
                    }
                }
                commands.Add(Commands.Wait());
            }
            s.DoTurn(commands); commands.Clear();

            foreach (Nanobot bot in s.Bots) {
                if (bot.Pos.Z < r - 1) {
                    if (bot.Pos.Z % 31 == (r - 1) % 31) {
                        commands.Add(Commands.FusionP(Delta.LinearZ(+1)));
                        continue;
                    }
                    if (bot.Pos.Z % 31 == (r - 0) % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearZ(-1)));
                        continue;
                    }
                }
                commands.Add(Commands.Wait());
            }
            s.DoTurn(commands); commands.Clear();

            while (true) {
                int x = 0;
                foreach (Nanobot bot in s.Bots)
                    x = Math.Max(bot.Pos.X, x);
                if (x == 0)
                    break;
                foreach (Nanobot bot in s.Bots) {
                    if (bot.Pos.X <= x - 2) {
                        commands.Add(Commands.Wait());
                        continue;
                    }
                    if (bot.Pos.X == x - 1) {
                        commands.Add(Commands.FusionP(Delta.LinearX(+1)));
                        continue;
                    }
                    if (x == 1 || x % 31 == r % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearX(-1)));
                    } else {
                        int dx = Math.Min(x - 1, 15);
                        commands.Add(Commands.SMove(Delta.LinearX(-dx)));
                    }
                }
                s.DoTurn(commands); commands.Clear();
            }

            while (true) {
                int z = 0;
                foreach (Nanobot bot in s.Bots)
                    z = Math.Max(bot.Pos.Z, z);
                if (z == 0)
                    break;
                foreach (Nanobot bot in s.Bots) {
                    if (bot.Pos.Z <= z - 2) {
                        commands.Add(Commands.Wait());
                        continue;
                    }
                    if (bot.Pos.Z == z - 1) {
                        commands.Add(Commands.FusionP(Delta.LinearZ(+1)));
                        continue;
                    }
                    if (z == 1 || z % 31 == r % 31) {
                        commands.Add(Commands.FusionS(Delta.LinearZ(-1)));
                    } else {
                        int dz = -Math.Min(z - 1, 15);
                        commands.Add(Commands.SMove(Delta.LinearZ(dz)));
                    }
                }
                s.DoTurn(commands); commands.Clear();
            }
        }

        private static bool IsNoop(IEnumerable<Command> commands)
            => commands.All(c => c == Commands.Wait());
    }
}
