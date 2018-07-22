using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public enum CommandType
{
    None,
    Halt,
    Wait,
    Flip,
    Smove,
    Lmove,
    FusionP,
    FusionS,
    Fission,
    Fill,
}

[System.Serializable]
public class Command
{
    [SerializeField] CommandType type;
    [SerializeField] Vector3Int diff1;
    [SerializeField] Vector3Int diff2;
    [SerializeField] int number;

    private Command()
    {
    }

    public CommandType Type { get { return type; } }
    public Vector3Int Diff1 { get { return diff1; } }
    public Vector3Int Diff2 { get { return diff2; } }
    public int Number { get { return number; } }

    public static Command None()
    {
        return new Command() { type = CommandType.None };
    }

    public static Command Halt()
    {
        return new Command() { type = CommandType.Halt };
    }

    public static Command Wait()
    {
        return new Command() { type = CommandType.Wait };
    }

    public static Command Flip()
    {
        return new Command() { type = CommandType.Flip };
    }

    public static Command Smove(Vector3Int diff)
    {
        return new Command() { type = CommandType.Smove, diff1 = diff };
    }

    public static Command Lmove(Vector3Int diff1, Vector3Int diff2)
    {
        return new Command() { type = CommandType.Lmove, diff1 = diff1, diff2 = diff2 };
    }

    public static Command FusionP(Vector3Int diff)
    {
        return new Command() { type = CommandType.FusionP, diff1 = diff };
    }

    public static Command FusionS(Vector3Int diff)
    {
        return new Command() { type = CommandType.FusionS, diff1 = diff };
    }

    public static Command Fission(Vector3Int diff, int number)
    {
        return new Command() { type = CommandType.Fill, diff1 = diff, number = number };
    }

    public static Command Fill(Vector3Int diff)
    {
        return new Command() { type = CommandType.Fill, diff1 = diff };
    }

    public void Write(BinaryWriter writer)
    {
        switch (type)
        {
            case CommandType.Halt:
                writer.Write((byte)0xFF);
                return;
            case CommandType.Wait:
                writer.Write((byte)0xFE);
                return;
            case CommandType.Flip:
                writer.Write((byte)0xFD);
                return;
            case CommandType.Smove:
                int axis = 0;
                int len = 0;
                if (diff1.x != 0)
                {
                    axis = 1;
                    len = EncodeLLD(diff1.x);
                } else if (diff1.y != 0)
                {
                    axis = 2;
                    len = EncodeLLD(diff1.y);
                } else
                {
                    axis = 3;
                    len = EncodeLLD(diff1.z);
                }
                writer.Write((byte)((axis << 4) | 4));
                writer.Write((byte)len);
                return;
            case CommandType.Lmove:
                int axis1 = 0;
                int axis2 = 0;
                int len1 = 0;
                int len2 = 0;
                if (diff1.x != 0)
                {
                    axis1 = 1;
                    len1 = EncodeSLD(diff1.x);
                } else if (diff1.y != 0)
                {
                    axis1 = 2;
                    len1 = EncodeSLD(diff1.y);
                } else
                {
                    axis1 = 3;
                    len1 = EncodeSLD(diff1.z);
                }
                if (diff2.x != 0)
                {
                    axis2 = 1;
                    len2 = EncodeSLD(diff2.x);
                } else if (diff2.y != 0)
                {
                    axis2 = 2;
                    len2 = EncodeSLD(diff2.y);
                } else
                {
                    axis2 = 3;
                    len2 = EncodeSLD(diff2.z);
                }
                writer.Write((byte)((axis2 << 6) | (axis1 << 4) | 12));
                writer.Write((byte)((len2 << 4) | len1));
                return;
            case CommandType.FusionP:
                writer.Write((byte)((EncodeND(diff1) << 3) | 7));
                return;
            case CommandType.FusionS:
                writer.Write((byte)((EncodeND(diff1) << 3) | 6));
                return;
            case CommandType.Fission:
                Assert.IsTrue(number < 20);
                writer.Write((byte)((EncodeND(diff1) << 3) | 5));
                writer.Write((byte)number);
                return;
            case CommandType.Fill:
                writer.Write((byte)((EncodeND(diff1) << 3) | 3));
                return;
        }
    }

    public void Write(TextWriter writer)
    {
        switch (type)
        {
            case CommandType.Halt:
            case CommandType.Wait:
            case CommandType.Flip:
                writer.WriteLine(type.ToString());
                return;
            case CommandType.FusionP:
            case CommandType.FusionS:
            case CommandType.Fill:
            case CommandType.Smove:
                writer.Write(type.ToString());
                writer.Write(' ');
                Write(writer, diff1);
                writer.WriteLine();
                return;
            case CommandType.Lmove:
                writer.Write(type.ToString());
                writer.Write(' ');
                Write(writer, diff1);
                writer.Write(' ');
                Write(writer, diff2);
                writer.WriteLine();
                return;
            case CommandType.Fission:
                writer.Write(type.ToString());
                writer.Write(' ');
                Write(writer, diff1);
                writer.Write(' ');
                writer.Write(number);
                writer.WriteLine();
                return;
        }
    }

    public static void Write(TextWriter writer, Vector3Int vector)
    {
        writer.Write('<');
        writer.Write(vector.x);
        writer.Write(',');
        writer.Write(vector.y);
        writer.Write(',');
        writer.Write(vector.z);
        writer.Write('>');
    }

    public static int EncodeLLD(int diff)
    {
        Assert.IsTrue(-15 <= diff && diff <= 15);
        return diff + 15;
    }

    public static int EncodeSLD(int diff)
    {
        Assert.IsTrue(-5 <= diff && diff <= 5);
        return diff + 5;
    }

    public static int EncodeND(Vector3Int diff)
    {
        Assert.IsTrue(-1 <= diff.x && diff.x <= 1);
        Assert.IsTrue(-1 <= diff.y && diff.y <= 1);
        Assert.IsTrue(-1 <= diff.z && diff.z <= 1);
        return (diff.x + 1) * 9 + (diff.y + 1) * 3 + (diff.z + 1);
    }
}
