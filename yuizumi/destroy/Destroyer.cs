using System;
using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    internal abstract partial class Destroyer
    {
        internal Destroyer(State state)
        {
            S = state;

            MinX = Int32.MaxValue; MaxX = Int32.MinValue;
            MinY = Int32.MaxValue; MaxY = Int32.MinValue;
            MinZ = Int32.MaxValue; MaxZ = Int32.MinValue;

            for (int x = 0; x < R; x++)
            for (int y = 0; y < R; y++)
            for (int z = 0; z < R; z++) {
                if (S.Matrix[x, y, z] == Voxel.Full) {
                    MinX = Math.Min(x, MinX); MaxX = Math.Max(x, MaxX);
                    MinY = Math.Min(y, MinY); MaxY = Math.Max(y, MaxY);
                    MinZ = Math.Min(z, MinZ); MaxZ = Math.Max(z, MaxZ);
                }
            }
        }

        protected readonly State S;

        protected readonly int MinX;
        protected readonly int MaxX;
        protected readonly int MinY;
        protected readonly int MaxY;
        protected readonly int MinZ;
        protected readonly int MaxZ;

        protected int R => S.Matrix.R;

        internal abstract void Solve();

        protected static bool IsNoop(IEnumerable<Command> commands)
            => commands.All(c => c == Commands.Wait());
    }
}
