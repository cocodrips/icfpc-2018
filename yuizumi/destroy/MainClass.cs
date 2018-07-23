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
            DestroyV1.Doit(state);
            state.SaveToNbt(args[1]);
            Console.WriteLine($"Energy: {state.Energy}");
        }
    }
}
