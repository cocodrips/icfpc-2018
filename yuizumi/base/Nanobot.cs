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

        internal Nanobot Fission(Coord pos, int m)
        {
            var newBot = new Nanobot(mSeeds[0], pos, mSeeds.Skip(1).Take(m));
            mSeeds.RemoveRange(0, m + 1);
            return newBot;
        }

        internal void Fusion(Nanobot oldBot)
        {
            int index = mSeeds.Count;
            for (; index > 0 && mSeeds[index - 1] > oldBot.Bid; --index) {}
            mSeeds.Insert(index, oldBot.Bid);
        }

        public override string ToString() => $"Nanobot({Bid})";
    }
}
