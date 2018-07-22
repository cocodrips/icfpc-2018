using System;
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

        public void DoTurn(IReadOnlyList<Command> commands)
        {
            Requires.State(mBots.Count > 0, "System has been halted.");

            Requires.NotNull(commands, nameof(commands));
            Requires.NoneNull(commands, nameof(commands));
            Requires.Arg(mBots.Count == commands.Count, nameof(commands),
                         "Sequence must have the length matching Bots.");

            IEnumerable<(Nanobot, Command)> assignments =
                mBots.Zip(commands, ValueTuple.Create).ToList();

            // Command pre-conditions that would lead to an error are checked.
            foreach ((Nanobot bot, Command command) in assignments)
                command.VerifyPreconds(this, bot);
            foreach ((Nanobot bot, Command command) in assignments)
                command.VerifyPartners(assignments, bot);

            // Interference between the volatile coordinates of nanobot groups that
            // would lead to an error are checked.
            var volatileSet = new HashSet<Coord>();
            foreach ((Nanobot bot, Command command) in assignments) {
                foreach (Coord c in command.GetVolatile(bot)) {
                    if (!volatileSet.Add(c)) {
                        throw new InvalidCommandException(command, $"Interfere at {c}.");
                    }
                }
            }

            // Given no errors, then the system state is updated.
            if (Harmonics == High) {
                Energy += 30 * Matrix.R * Matrix.R * Matrix.R;
            } else {
                Energy += 3  * Matrix.R * Matrix.R * Matrix.R;
            }
            Energy += 20 * mBots.Count;
            foreach ((Nanobot bot, Command command) in assignments)
                command.ApplyToState(this, bot);

            // TODO(yuizumi): Ensure the sate is well-formed.
        }
    }
}
