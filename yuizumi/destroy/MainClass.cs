using System;

namespace Yuizumi.Icfpc2018
{
    internal static class MainClass
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
                throw new Exception();

            var state = new RecordedState(ModelFile.Load(args[0]));
            state.DoesAutoVerify = false;
            if (state.Matrix.R <= 90) {
                new Planer(state).Solve();
            } else {
                new Linear(state).Solve();
            }
            state.SaveToNbt(args[1]);
            Console.WriteLine($"Energy: {state.Energy}");
        }
    }
}
