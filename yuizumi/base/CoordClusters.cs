using System;
using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    internal class CoordClusters
    {
        internal CoordClusters() {}

        private readonly Dictionary<Coord, int> mCoordIds =
            new Dictionary<Coord, int>();
        private readonly List<int> mRoot = new List<int>() { 0 };
        private readonly List<int> mSize = new List<int>() { 1 };

        internal int Count { get; private set; } = 1;

        internal void Add(Coord c)
        {
            int k = mRoot.Count;
            mCoordIds.Add(c, k);
            mRoot.Add(k);
            mSize.Add(1);
            if (c.Y == 0) Unite(k, 0);
            ++Count;
        }

        internal void Unite(Coord c1, Coord c2)
        {
            Unite(mCoordIds[c1], mCoordIds[c2]);
        }

        private void Unite(int k1, int k2)
        {
            int r1 = GetRoot(k1);
            int r2 = GetRoot(k2);
            if (r1 == r2)
                return;
            --Count;
            if (mSize[r1] > mSize[r2]) {
                mRoot[r2] = r1;
                mSize[r1] += mSize[r2];
            } else {
                mRoot[r1] = r2;
                mSize[r2] += mSize[r1];
            }
        }

        private int GetRoot(int id)
        {
            return (mRoot[id] == id) ? id : (mRoot[id] = GetRoot(mRoot[id]));
        }
    }
}
