using System;

namespace Yuizumi.Icfpc2018
{
    internal static class DeltaDecoder
    {
        internal static Delta DecodeLld(int a, int i)
            => DecodeLd(a, i, 15);

        internal static Delta DecodeSld(int a, int i)
            => DecodeLd(a, i, 5);

        private static Delta DecodeLd(int a, int i, int min)
        {
            switch (a) {
                case 1: return Delta.LinearX(i - min);
                case 2: return Delta.LinearY(i - min);
                case 3: return Delta.LinearZ(i - min);

                default:
                    throw new ArgumentOutOfRangeException(nameof(a));
            }

        }

        internal static Delta DecodeNd(int nd)
        {
            Requires.Range(nd, nameof(nd), 0, 26);

            int dx = (nd / 9) % 3 - 1;
            int dy = (nd / 3) % 3 - 1;
            int dz = (nd / 1) % 3 - 1;

            Delta decoded = Delta.Of(dx, dy, dz);
            Requires.Arg(decoded.IsNd(), nameof(nd),
                         $"{nd} is not a valid encoding of a near difference.");
            return decoded;
        }

        internal static Delta DecodeFd(int dx, int dy, int dz)
        {
            Requires.Range(dx, nameof(dx), 0, 60);
            Requires.Range(dy, nameof(dy), 0, 60);
            Requires.Range(dz, nameof(dz), 0, 60);
            dx -= 30; dy -= 30; dz -= 30;
            return Delta.Of(dx, dy, dz);
        }
    }
}

