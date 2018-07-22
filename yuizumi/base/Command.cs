using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    using Assignments = IEnumerable<(Nanobot, Command)>;

    public abstract class Command
    {
        internal abstract IEnumerable<byte> Encode();

        internal virtual void VerifyPreconds(State state, Nanobot bot) {}
        internal virtual void VerifyPartners(
            Assignments assignments, Nanobot bot) {}

        internal abstract IEnumerable<Coord> GetVolatile(Nanobot bot);
        internal abstract void ApplyToState(State state, Nanobot bot);
    }
}
