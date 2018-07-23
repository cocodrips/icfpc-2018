using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Yuizumi.Icfpc2018
{
    public static class TraceFile
    {
        public static IEnumerable<Command> Load(string filename)
        {
            Requires.NotNull(filename, nameof(filename));
            using (var stream = File.OpenRead(filename))
                foreach (Command c in Load(stream)) yield return c;
        }

        public static IEnumerable<Command> Load(Stream stream)
        {
            Requires.NotNull(stream, nameof(stream));
            Command command;
            while ((command = ReadCommand(stream)) != null)
                yield return command;
        }

        public static void Save(string filename, IEnumerable<Command> trace)
        {
            Requires.NotNull(filename, nameof(filename));
            using (var stream = File.OpenWrite(filename))
                Save(stream, trace);
        }

        public static void Save(Stream stream, IEnumerable<Command> trace)
        {
            Requires.NotNull(stream, nameof(stream));
            Requires.NotNull(trace, nameof(trace));

            foreach (Command command in trace) {
                if (command == null)
                    continue;
                foreach (byte b in command.Encode())
                    stream.WriteByte(b);
            }
        }

        public static IEnumerable<Command> LoadText(string filename)
        {
            Requires.NotNull(filename, nameof(filename));
            using (var reader = new StreamReader(filename))
                foreach (Command c in LoadText(reader)) yield return c;
        }

        public static IEnumerable<Command> LoadText(TextReader reader)
        {
            Requires.NotNull(reader, nameof(reader));

            int lineNumber = 0;
            string line;
            while ((line = reader.ReadLine()) != null) {
                Command command = ParseLine(line, ++lineNumber);
                if (command != null) yield return command;
            }
        }

        public static void SaveText(string filename, IEnumerable<Command> trace)
        {
            Requires.NotNull(filename, nameof(filename));
            using (var writer = new StreamWriter(filename))
                SaveText(writer, trace);
        }

        public static void SaveText(TextWriter writer, IEnumerable<Command> trace)
        {
            Requires.NotNull(writer, nameof(writer));
            Requires.NotNull(trace, nameof(trace));

            foreach (Command command in trace)
                writer.WriteLine(command);
        }

        private static Command ReadCommand(Stream stream)
        {
            int prefix = stream.ReadByte();
            if (prefix == -1)
                return null;
            Decoder decoder = Commands.Decoders.SingleOrDefault(
                d => d.CanDecode(prefix));
            if (decoder == null) {
                throw new IOException("Stream is not a valid binary trace.");
            }
            try {
                return decoder.Decode(prefix, stream.StrictReadByte);
            } catch (ArgumentException) {
                throw new IOException("Stream is not a valid binary trace.");
            }
        }

        private static readonly Regex TokenRegex =
            new Regex("[A-Za-z]+|[0-9]+|<[^<>]*>");

        private static readonly Regex LineRegex =
            new Regex("^(?:[A-Za-z]+|[0-9]+|<[^<>]*>|\\s+)*$");

        private static Command ParseLine(string line, int lineNumber)
        {
            line = Regex.Replace(line, "#.*$", "").Trim();
            if (line == "") return null;

            if (!LineRegex.IsMatch(line)) {
                throw new IOException($"Syntax error at Line {lineNumber}.");
            }

            List<string> tokens = TokenRegex.Matches(line).Cast<Match>()
                .Select(m => m.Value).ToList();

            Decoder decoder = Commands.Decoders.SingleOrDefault(
                d => tokens[0] == d.Name);
            if (decoder == null) {
                throw new IOException($"Syntax error at Line {lineNumber}.");
            }
            tokens.RemoveAt(0);  // Oops...
            try {
                return decoder.DecodeText(tokens);
            } catch (ArgumentException) {
                throw new IOException($"Syntax error at Line {lineNumber}.");
            }
        }
    }
}
