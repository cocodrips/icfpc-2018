using System;

namespace Yuizumi.Icfpc2018
{
    public struct Coord : IEquatable<Coord>
    {
        private Coord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Coord Zero => default(Coord);

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public static bool operator==(Coord c1, Coord c2)
            => c1.Equals(c2);

        public static bool operator!=(Coord c1, Coord c2)
            => !c1.Equals(c2);

        public static Coord Of(int x, int y, int z)
            => new Coord(x, y, z);

        public bool Equals(Coord that)
        {
            return (this.X == that.X) && (this.Y == that.Y) &&
                (this.Z == that.Z);
        }

        public override bool Equals(object obj)
            => (obj is Coord c) && Equals(c);

        public override int GetHashCode() => (X, Y, Z).GetHashCode();

        public override string ToString() => $"({X},{Y},{Z})";
    }
}
