using static System.Math;

namespace Yuizumi.Icfpc2018
{
    public static class DeltaExtension
    {
        private static int Max3(int a, int b, int c)
            => Max(a, Max(b, c));

        public static int Mlen(this Delta d)
        {
            return Abs(d.DX) + Abs(d.DY) + Abs(d.DZ);
        }

        public static int Clen(this Delta d)
        {
            return Max3(Abs(d.DX), Abs(d.DY), Abs(d.DZ));
        }

        public static bool IsLd(this Delta d)
        {
            return (d.DX != 0 && d.DY == 0 && d.DZ == 0)
                || (d.DX == 0 && d.DY != 0 && d.DZ == 0)
                || (d.DX == 0 && d.DY == 0 && d.DZ != 0);
        }

        public static bool IsSld(this Delta d)
            => IsLd(d) && Mlen(d) <= 5;

        public static bool IsLld(this Delta d)
            => IsLd(d) && Mlen(d) <= 15;

        public static bool IsNd(this Delta d)
            => Mlen(d) <= 2 && Clen(d) == 1;

        public static bool IsFd(this Delta d)
            => 0 < Clen(d) && Clen(d) <= 30;
    }
}
