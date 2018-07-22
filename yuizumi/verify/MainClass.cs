using System;
using System.Collections.Generic;
using System.IO;

namespace Yuizumi.Icfpc2018
{
    internal static class MainClass
    {
        private const int MaxVerifyR = 50;

        private static int Main(string[] args)
        {
            try {
                ActualMain(args);
                return 0;
            } catch (CommandLineException e) {
                Console.Error.WriteLine($"{GetProgramName()}: {e.Message}");
                return 1;
            } catch (IOException e) {
                Console.Error.WriteLine($"{GetProgramName()}: {e.Message}");
                return 1;
            } catch (InvalidOperationException e) {
                Console.Error.WriteLine($"{GetProgramName()}: {e.Message}");
                return 1;
            }
        }

        private static string GetProgramName()
        {
            return Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        }

        private static void ActualMain(string[] args)
        {
            if (args.Length != 3) {
                throw new CommandLineException(
                    $"Usage: {GetProgramName()} YOUR_NBT SOURCE_MDL TARGET_MDL");
            }

            Matrix source = null;
            Matrix target = null;

            if (args[1] != "-") {
                using (Stream stream = new FileStream(args[1], FileMode.Open))
                    source = ModelFile.Load(stream);
            }
            if (args[2] != "-") {
                using (Stream stream = new FileStream(args[2], FileMode.Open))
                    target = ModelFile.Load(stream);
            }

            if (source == null) source = new Matrix(target.R);
            if (target == null) target = new Matrix(source.R);

            var commands = new List<Command>();
            var state = new State(source);

            using (Stream stream = new FileStream(args[0], FileMode.Open)) {
                foreach (Command c in NbtFile.Load(stream)) {
                    commands.Add(c);
                    if (commands.Count == state.Bots.Count) {
                        state.DoTurn(commands);
                        if (state.Matrix.R <= MaxVerifyR) state.VerifyWellFormed();
                        commands.Clear();
                    }
                }
            }

            if (!Matrix.AreEqual(state.Matrix, target)) {
                throw new InvalidOperationException("Matrix does not match the target.");
            }

            Console.WriteLine(state.Energy);
        }
    }
}
