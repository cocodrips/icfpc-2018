using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public static class MatrixExtension
    {
        public static bool IsGrounded(this Matrix m)
        {
            var clusters = new CoordClusters();

            for (int y = 0; y < m.R; y++)
            for (int x = 0; x < m.R; x++)
            for (int z = 0; z < m.R; z++) {
                if (m[x, y, z] == Voxel.Void)
                    continue;
                clusters.Add(Coord.Of(x, y, z));
                if (x > 0 && m[x - 1, y, z] == Voxel.Full)
                    clusters.Unite(Coord.Of(x, y, z), Coord.Of(x - 1, y, z));
                if (y > 0 && m[x, y - 1, z] == Voxel.Full)
                    clusters.Unite(Coord.Of(x, y, z), Coord.Of(x, y - 1, z));
                if (z > 0 && m[x, y, z - 1] == Voxel.Full)
                    clusters.Unite(Coord.Of(x, y, z), Coord.Of(x, y, z - 1));
            }

            return clusters.Count == 1;
        }
    }
}
