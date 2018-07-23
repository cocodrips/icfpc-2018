namespace Yuizumi.Icfpc2018
{
    public class Matrix
    {
        private Matrix(int r)
        {
            mVoxels = new Voxel[r, r, r];
            R = r;
        }

        private readonly Voxel[,,] mVoxels;

        public int R { get; }

        public Voxel this[int x, int y, int z]
        {
            get {
                Requires.Range(x, nameof(x), 0, R - 1);
                Requires.Range(y, nameof(y), 0, R - 1);
                Requires.Range(z, nameof(z), 0, R - 1);
                return mVoxels[x, y, z];
            }
            set {
                Requires.Range(x, nameof(x), 0, R - 1);
                Requires.Range(y, nameof(y), 0, R - 1);
                Requires.Range(z, nameof(z), 0, R - 1);
                mVoxels[x, y, z] = value;
            }
        }

        public Voxel this[Coord c]
        {
            get {
                Requires.Arg(Contains(c), nameof(c), $"{c} is not a valid coordinate.");
                return mVoxels[c.X, c.Y, c.Z];
            }
            set {
                Requires.Arg(Contains(c), nameof(c), $"{c} is not a valid coordinate.");
                mVoxels[c.X, c.Y, c.Z] = value;
            }
        }

        public static Matrix Empty(int r)
            => new Matrix(r);

        public bool Contains(Coord c)
        {
            return (c.X >= 0 && c.X < R)
                && (c.Y >= 0 && c.Y < R)
                && (c.Z >= 0 && c.Z < R);
        }

        public static bool AreEqual(Matrix m1, Matrix m2)
        {
            if (m1.R != m2.R) return false;

            for (int x = 0; x < m1.R; x++)
            for (int y = 0; y < m1.R; y++)
            for (int z = 0; z < m1.R; z++) {
                if (m1.mVoxels[x, y, z] != m2.mVoxels[x, y, z])
                    return false;
            }
            return true;
        }

        public override string ToString() => $"Matrix({R})";
    }
}
