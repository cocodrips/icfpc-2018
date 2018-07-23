using System.Collections.Generic;

namespace Yuizumi.Icfpc2018
{
    public class RecordedState : State
    {
        public RecordedState(Matrix matrix)
            : base(matrix) {}

        private readonly List<Command> mTrace = new List<Command>();

        public override void DoTurn(IReadOnlyList<Command> commands)
        {
            base.DoTurn(commands);
            mTrace.AddRange(commands);
        }

        public void SaveToNbt(string filename)
            => TraceFile.Save(filename, mTrace);
    }
}
