using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public struct Region : IEquatable<Region>
    {
        private Region(Coord c1, Coord c2)
        {
            C1 = c1;
            C2 = c2;
        }

        public Coord C1 { get; }
        public Coord C2 { get; }

        public int MinX => Math.Min(C1.X, C2.X);
        public int MaxX => Math.Max(C1.X, C2.X);
        public int MinY => Math.Min(C1.Y, C2.Y);
        public int MaxY => Math.Max(C1.Y, C2.Y);
        public int MinZ => Math.Min(C1.Z, C2.Z);
        public int MaxZ => Math.Max(C1.Z, C2.Z);

        public static Region Of(Coord c1, Coord c2)
            => new Region(c1, c2);

        public IEnumerable<Coord> GetMembers()
        {
            for (int x = MinX; x <= MaxX; x++)
            for (int y = MinY; y <= MaxY; y++)
            for (int z = MinZ; z <= MaxZ; z++)
                yield return Coord.Of(x, y ,z);
        }

        public bool Equals(Region that)
        {
            return (this.MinX == that.MinX && this.MaxX == that.MaxX)
                && (this.MinY == that.MinY && this.MaxY == that.MaxY)
                && (this.MinZ == that.MinZ && this.MaxZ == that.MaxZ);
        }

        public override bool Equals(object obj)
            => (obj is Region r) && Equals(r);

        public override int GetHashCode()
            => (MinX, MaxX, MinY, MaxY, MinZ, MaxZ).GetHashCode();

        public override string ToString() => $"[{C1}, {C2}]";
    }
}
