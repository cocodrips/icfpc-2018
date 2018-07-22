using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    public class Nanobot
    {
        internal Nanobot(int bid, Coord pos, IEnumerable<int> seeds)
        {
            Bid = bid;
            Pos = pos;
            mSeeds = seeds.ToList();
        }

        private readonly List<int> mSeeds;

        public int Bid { get; }

        public Coord Pos { get; internal set; }

        public IReadOnlyList<int> Seeds
            => mSeeds.AsReadOnly();

        public override string ToString() => $"Nanobot({Bid})";
    }
}
