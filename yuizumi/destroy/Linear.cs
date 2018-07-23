using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    internal class Linear : Destroyer
    {
        internal Linear(State state) : base(state) {}

        internal override void Solve()
        {
            MoveToY(MaxY + 1);
            Prepare();
            Flip();
            Execute();
            Flip();
            Cleanup();
            S.DoTurn(new [] {Commands.Halt()});
        }

        private void Flip()
        {
            var commands = new Command[S.Bots.Count];
            for (int i = 0; i < S.Bots.Count; i++)
                commands[i] = (i == 0) ? Commands.Flip() : Commands.Wait();
            S.DoTurn(commands);
        }

        private void Prepare()
        {
            var coords = new List<int>();

            for (int i = R - 1; i > 0; i -= 31) {
                coords.Add(i);
                coords.Add(Math.Max(i - 30, 0));
            }

            int n = coords.Count;

            var dests = new List<Coord>() {
                Coord.Of(0, MaxY + 1, 0),
                Coord.Of(coords[0], MaxY + 1, 30),
            };
            for (int i = 1; i < n; i++) {
                dests.Add(Coord.Of(coords[i - 0], MaxY + 1, 30));
                dests.Add(Coord.Of(coords[i - 1], MaxY + 1, 0));
            }

            LocateBots(dests);
        }

        private void Execute()
        {
            var commands = new Command[S.Bots.Count];
            Delta down = Delta.LinearY(-1);

            while (true) {
                int z = 0;

                while (true) {
                    for (int i = 0; i < S.Bots.Count; i++) {
                        int dx = (S.Bots[i].Pos.X % 31 == (R - 1) % 31) ? -30 : +30;
                        if (S.Bots[i].Pos.X == 0) dx = (R + 30) % 31;
                        dx = Math.Max(dx, -S.Bots[i].Pos.X);
                        int dz = (S.Bots[i].Pos.Z == z) ? +30 : -30;
                        commands[i] = Commands.GVoid(down, Delta.Of(dx, 0, dz));
                    }
                    S.DoTurn(commands);

                    if (z + 30 == MaxZ) break;
                    
                    int dz0 = Math.Min(z + 30, MaxZ - 30) - z;
                    z += dz0;

                    for (int dz = dz0; dz > 0; dz -= 15) {
                        for (int i = 0; i < S.Bots.Count; i++) {
                            commands[i] = Commands.SMove(Delta.LinearZ(Math.Min(dz, 15)));
                        }
                        S.DoTurn(commands);
                    }
                }

                for (int i = 0; i < S.Bots.Count; i++)
                    commands[i] = Commands.SMove(down);
                S.DoTurn(commands);

                if (S.Bots[0].Pos.Y == 0) {
                    for (; z > 0; z -= 15) {
                        for (int i = 0; i < S.Bots.Count; i++) {
                            commands[i] = Commands.SMove(Delta.LinearZ(-Math.Min(z, 15)));
                        }
                        S.DoTurn(commands);
                    }
                    break;
                }

                while (true) {
                    for (int i = 0; i < S.Bots.Count; i++) {
                        int dx = (S.Bots[i].Pos.X % 31 == (R - 1) % 31) ? -30 : +30;
                        if (S.Bots[i].Pos.X == 0) dx = (R + 30) % 31;
                        dx = Math.Max(dx, -S.Bots[i].Pos.X);
                        int dz = (S.Bots[i].Pos.Z == z) ? +30 : -30;
                        commands[i] = Commands.GVoid(down, Delta.Of(dx, 0, dz));
                    }
                    S.DoTurn(commands);

                    if (z == 0) break;

                    int dz0 = z - Math.Max(z - 30, 0);
                    z -= dz0;

                    for (int dz = dz0; dz > 0; dz -= 15) {
                        for (int i = 0; i < S.Bots.Count; i++) {
                            commands[i] = Commands.SMove(Delta.LinearZ(-Math.Min(dz, 15)));
                        }
                        S.DoTurn(commands);
                    }
                }

                for (int i = 0; i < S.Bots.Count; i++)
                    commands[i] = Commands.SMove(down);
                S.DoTurn(commands);

                if (S.Bots[0].Pos.Y == 0) break;
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
        }
    }
}
