using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    internal partial class Destroyer
    {
        protected void MoveToY(int y)
        {
            while (S.Bots[0].Pos.Y < y) {
                int dy = y - S.Bots[0].Pos.Y;
                S.DoTurn(new [] {Commands.SMove(Delta.LinearY(Math.Min(dy, 15)))});
            }
        }

        protected void LocateBots(IReadOnlyList<Coord> dests)
        {
            var commands = new List<Command>();

            while (true) {
                if (S.Bots.Count < dests.Count) {
                    int dx = (S.Bots.Count % 2 == 1) ? 1 : 0;
                    int dz = (S.Bots.Count % 2 == 1) ? 0 : 1;
                    commands.Add(Commands.Fission(Delta.Of(dx, 0, dz), 0));
                } else {
                    commands.Add(Commands.Wait());
                }

                for (int i = 1; i < S.Bots.Count; i++) {
                    int dx = dests[i].X - S.Bots[i].Pos.X;
                    int dz = dests[i].Z - S.Bots[i].Pos.Z;
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
                S.DoTurn(commands);
                commands.Clear();
            }
        }
    }
}
