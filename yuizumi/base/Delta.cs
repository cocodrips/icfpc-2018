using System;
using System.Text.RegularExpressions;

namespace Yuizumi.Icfpc2018
{
    public struct Delta : IEquatable<Delta>
    {
        private Delta(int dx, int dy, int dz)
        {
            DX = dx;
            DY = dy;
            DZ = dz;
        }

        private static readonly Regex ParserRegex = GetParserRegex();

        public static Delta Zero => default(Delta);

        public int DX { get; }
        public int DY { get; }
        public int DZ { get; }

        public static bool operator==(Delta d1, Delta d2)
            => d1.Equals(d2);

        public static bool operator!=(Delta d1, Delta d2)
            => !d1.Equals(d2);

        public static Coord operator+(Coord c, Delta d)
        {
            return Coord.Of(c.X + d.DX, c.Y + d.DY, c.Z + d.DZ);
        }

        public static Delta Of(int dx, int dy, int dz)
            => new Delta(dx, dy, dz);

        public static Delta LinearX(int d)
        {
            Requires.Arg(d != 0, nameof(d), "Argument must be non-zero.");
            return new Delta(d, 0, 0);
        }

        public static Delta LinearY(int d)
        {
            Requires.Arg(d != 0, nameof(d), "Argument must be non-zero.");
            return new Delta(0, d, 0);
        }

        public static Delta LinearZ(int d)
        {
            Requires.Arg(d != 0, nameof(d), "Argument must be non-zero.");
            return new Delta(0, 0, d);
        }

        public static Delta Parse(string text)
        {
            Match match = ParserRegex.Match(text);
            if (!match.Success) {
                throw new FormatException();
            }
            int dx = Int32.Parse(match.Groups[1].Value);
            int dy = Int32.Parse(match.Groups[2].Value);
            int dz = Int32.Parse(match.Groups[3].Value);
            return Delta.Of(dx, dy, dz);
        }

        private static Regex GetParserRegex()
        {
            string number = @"\s*(-?\d+)\s*";
            return new Regex($"^<{number},{number},{number}>$");
        }

        public bool Equals(Delta that)
        {
            return (this.DX == that.DX) && (this.DY == that.DY) &&
                (this.DZ == that.DZ);
        }

        public override bool Equals(object obj)
            => (obj is Delta d) && Equals(d);

        public override int GetHashCode() => (DX, DY, DZ).GetHashCode();

        public override string ToString() => $"<{DX},{DY},{DZ}>";
    }
}
