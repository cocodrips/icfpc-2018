using System.Collections.Generic;
using System.Linq;

namespace Yuizumi.Icfpc2018
{
    using static Harmonics;

    public class State
    {
        public State(Matrix matrix)
        {
            Energy = 0;
            Harmonics = Low;
            Matrix = matrix;

            mBots = new List<Nanobot>() {
                new Nanobot(1, Coord.Zero, Enumerable.Range(2, count: 39)),
            };
        }

        private readonly List<Nanobot> mBots;

        public long Energy { get; internal set; }

        public Harmonics Harmonics { get; internal set; }

        public Matrix Matrix { get; }

        public IReadOnlyList<Nanobot> Bots
            => mBots.AsReadOnly();
    }
}
