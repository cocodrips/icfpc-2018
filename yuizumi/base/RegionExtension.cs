namespace Yuizumi.Icfpc2018
{
    public static class RegionExtension
    {
        public static int Dim(this Region r)
        {
            return (r.C1.X == r.C2.X ? 0 : 1) + (r.C1.Y == r.C2.Y ? 0 : 1) +
                (r.C1.Z == r.C2.Z ? 0 : 1);
        }
    }
}

