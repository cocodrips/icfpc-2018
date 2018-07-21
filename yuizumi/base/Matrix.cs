namespace Yuizumi.Icfpc2018
{
    public class Matrix
    {
        public Matrix(int r)
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

        public bool Contains(Coord c)
        {
            return (c.X >= 0 && c.X < R)
                && (c.Y >= 0 && c.Y < R)
                && (c.Z >= 0 && c.Z < R);
        }

        public override string ToString() => $"Matrix({R})";
    }
}
