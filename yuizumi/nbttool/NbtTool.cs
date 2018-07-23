using System;
using System.IO;

namespace Yuizumi.Icfpc2018
{
    internal static class NbtTool
    {
        private class UsageErrorException : Exception {}

        private static int Main(string[] args)
        {
            try {
                switch (args.Length) {
                    case 1:
                        Doit(args[0], "-", "-");
                        return 0;
                    case 2:
                        Doit(args[0], args[1], "-");
                        return 0;
                    case 3:
                        Doit(args[0], args[1], args[2]);
                        return 0;
                    default:
                        throw new UsageErrorException();
                }
            } catch (IOException e) {
                Console.Error.WriteLine($"{ProgramName}: {e.Message}");
                return 1;
            } catch (UsageErrorException) {
                Console.Error.WriteLine(
                    $"Usage: {ProgramName} {{decode|encode}} [INFILE [OUTFILE]]");
                return 1;
            }
        }

        private static string ProgramName
            => Path.GetFileName(Environment.GetCommandLineArgs()[0]);

        private static void Doit(string action, string source, string output)
        {
            switch (action) {
                case "decode":
                    Decode(source, output); break;
                case "encode":
                    Encode(source, output); break;
                default:
                    throw new UsageErrorException();
            }
        }

        private static void Decode(string sourceFile, string outputFile)
        {
            using (var source = OpenSourceStream(sourceFile))
            using (var output = OpenOutputWriter(outputFile))
                TraceFile.SaveText(output, TraceFile.Load(source));
        }

        private static void Encode(string sourceFile, string outputFile)
        {
            using (var source = OpenSourceReader(sourceFile))
            using (var output = OpenOutputStream(outputFile))
                TraceFile.Save(output, TraceFile.LoadText(source));
        }

        private static Stream OpenSourceStream(string filename)
        {
            return (filename == "-") ? Console.OpenStandardInput()
                                     : File.OpenRead(filename);
        }

        private static Stream OpenOutputStream(string filename)
        {
            return (filename == "-") ? Console.OpenStandardOutput()
                                     : File.OpenWrite(filename);
        }

        private static TextReader OpenSourceReader(string filename)
        {
            return (filename == "-") ? Console.In  : (new StreamReader(filename));
        }

        private static TextWriter OpenOutputWriter(string filename)
        {
            return (filename == "-") ? Console.Out : (new StreamWriter(filename));
        }
    }
}
