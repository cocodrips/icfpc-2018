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

            if (args[1] != "-") source = ModelFile.Load(args[1]);
            if (args[2] != "-") target = ModelFile.Load(args[2]);
            if (source == null) source = Matrix.Empty(target.R);
            if (target == null) target = Matrix.Empty(source.R);

            var commands = new List<Command>();
            var state = new State(source);

            state.DoesAutoVerify = source.R <= MaxVerifyR;

            foreach (Command c in TraceFile.Load(args[0])) {
                commands.Add(c);
                if (commands.Count == state.Bots.Count) {
                    state.DoTurn(commands);
                    commands.Clear();
                }
            }

            if (!Matrix.AreEqual(state.Matrix, target)) {
                throw new InvalidOperationException("Matrix does not match the target.");
            }
            if (state.Bots.Count > 0) {
                throw new InvalidOperationException("System has not been halted.");
            }

            Console.WriteLine(state.Energy);
        }
    }
}
