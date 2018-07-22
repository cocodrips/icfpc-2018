using System;
using System.Collections.Generic;
using System.IO;

namespace Yuizumi.Icfpc2018
{
    public static class NbtFile
    {
        public static void Save(Stream stream, IEnumerable<Command> trace)
        {
            Requires.NotNull(stream, nameof(stream));
            Requires.NotNull(trace, nameof(trace));
            Requires.NoneNull(trace, nameof(trace));

            foreach (Command command in trace)
            foreach (byte b in command.Encode())
                stream.WriteByte(b);
        }

        public static IEnumerable<Command> Load(Stream stream)
        {
            Requires.NotNull(stream, nameof(stream));

            while (true) {
                Command c = null;
                try {
                    c = ReadCommand(stream);
                } catch (ArgumentException e) {
                    throw new IOException(
                        $"Stream is not a valid binary trace -- {e.Message}", e);
                }
                if (c == null) break;
                yield return c;
            }
        }

        private static Command ReadCommand(Stream stream)
        {
            int b1 = stream.ReadByte();

            switch (b1) {
                case -1: return null;

                case 0b11111111:
                    return Commands.Halt();
                case 0b11111110:
                    return Commands.Wait();
                case 0b11111101:
                    return Commands.Flip();
            }

            if ((b1 & 0b11001111) == 0b00000100)
                return ReadSMove(b1, stream);
            if ((b1 & 0b00001111) == 0b00001100)
                return ReadLMove(b1, stream);

            switch (b1 & 0b00000111) {
                case 0b111:
                    return ReadFusionP(b1, stream);
                case 0b100:
                    return ReadFusionS(b1, stream);
                case 0b101:
                    return ReadFission(b1, stream);
                case 0b011:
                    return ReadFill(b1, stream);
                case 0b010:
                    return ReadVoid(b1, stream);
                case 0b001:
                    return ReadGFill(b1, stream);
                case 0b000:
                    return ReadGVoid(b1, stream);
            }

            throw new IOException($"Invalid first byte: {b1}.");
        }

        private static Command ReadSMove(int b1, Stream stream)
        {
            int b2 = stream.StrictReadByte();
            int a = (b1 & 0b00110000) >> 4;
            int i = (b2 & 0b00011111) >> 0;
            Delta lld = DeltaDecoder.DecodeLld(a, i);
            return Commands.SMove(lld);
        }

        private static Command ReadLMove(int b1, Stream stream)
        {
            int b2 = stream.StrictReadByte();
            int a1 = (b1 & 0b00110000) >> 4;
            int i1 = (b2 & 0b00001111) >> 0;
            int a2 = (b1 & 0b11000000) >> 6;
            int i2 = (b2 & 0b11110000) >> 4;
            Delta sld1 = DeltaDecoder.DecodeSld(a1, i1);
            Delta sld2 = DeltaDecoder.DecodeSld(a2, i2);
            return Commands.LMove(sld1, sld2);
        }

        private static Command ReadFusionP(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            Delta nd = DeltaDecoder.DecodeNd(n);
            return Commands.FusionP(nd);
        }

        private static Command ReadFusionS(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            Delta nd = DeltaDecoder.DecodeNd(n);
            return Commands.FusionS(nd);
        }

        private static Command ReadFission(int b1, Stream stream)
        {
            int b2 = stream.StrictReadByte();
            int n = (b1 & 0b11111000) >> 3;
            int m = (b2 & 0b00000000) >> 0;
            Delta nd = DeltaDecoder.DecodeNd(n);
            return Commands.Fission(nd, m);
        }

        private static Command ReadFill(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            Delta nd = DeltaDecoder.DecodeNd(n);
            return Commands.Fill(nd);
        }

        private static Command ReadVoid(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            Delta nd = DeltaDecoder.DecodeNd(n);
            return Commands.Void(nd);
        }

        private static Command ReadGFill(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            int dx = stream.StrictReadByte();
            int dy = stream.StrictReadByte();
            int dz = stream.StrictReadByte();
            Delta nd = DeltaDecoder.DecodeNd(n);
            Delta fd = DeltaDecoder.DecodeFd(dx, dy, dz);
            return Commands.GFill(nd, fd);
        }

        private static Command ReadGVoid(int b1, Stream stream)
        {
            int n = (b1 & 0b11111000) >> 3;
            int dx = stream.StrictReadByte();
            int dy = stream.StrictReadByte();
            int dz = stream.StrictReadByte();
            Delta nd = DeltaDecoder.DecodeNd(n);
            Delta fd = DeltaDecoder.DecodeFd(dx, dy, dz);
            return Commands.GVoid(nd, fd);
        }
    }
}
