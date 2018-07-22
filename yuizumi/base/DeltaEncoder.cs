using System;

namespace Yuizumi.Icfpc2018
{
    internal static class DeltaEncoder
    {
        internal static (int a, int i) EncodeLld(Delta lld)
            => EncodeLd(lld, 15);

        internal static (int a, int i) EncodeSld(Delta sld)
            => EncodeLd(sld, 5);

        private static (int a, int i) EncodeLd(Delta ld, int min)
        {
            if (ld.DX != 0 && ld.DY == 0 && ld.DZ == 0)
                return (1, ld.DX + min);
            if (ld.DX == 0 && ld.DY != 0 && ld.DZ == 0)
                return (2, ld.DY + min);
            if (ld.DX == 0 && ld.DY == 0 && ld.DZ != 0)
                return (3, ld.DZ + min);

            throw new ArgumentException("Argument is not a valid ld.", nameof(ld));
        }

        internal static int EncodeNd(Delta nd)
        {
            return (nd.DX + 1) * 9 + (nd.DY + 1) * 3 + (nd.DZ + 1);
        }

        internal static (int, int, int) EncodeFd(Delta fd)
        {
            return (fd.DX + 30, fd.DY + 30, fd.DZ + 30);
        }
    }
}
