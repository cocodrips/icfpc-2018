using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class State
{
    const int fillBit = 0;
    const int modelBit = 1;

    [SerializeField] int time;
    [SerializeField] long energy;
    [SerializeField] bool harmonics;
    [SerializeField] int resolution;
    [SerializeField] List<Bot> bots;
    [SerializeField] int executed;
    [SerializeField] List<Command> trace;
    [SerializeField] byte[] matrix;

    public int Resolution { get { return resolution; } }

    public int Volume { get { return resolution * resolution * resolution; } }

    public long Energy { get { return energy; } }

    public bool IsHighHarmonics { get { return harmonics; } }

    public List<Bot> Bots { get { return bots; } }

    public void AddCommand(Command command)
    {
        trace.Add(command);
    }

    public void AddCommand(IEnumerable<Command> commands)
    {
        trace.AddRange(commands);
    }

    public void Step()
    {
        Assert.IsTrue(trace.Count - executed >= bots.Count);
        int initialBotsCount = bots.Count;
        for (int i = 0; i < initialBotsCount; i++)
        {
            Command command = trace [executed + i];
            Bot bot = bots [i];
            UncheckedRun(bot, command);
            bot.LastCommand = command;
        }
        bots.RemoveAll(x => x.LastCommand.Type == CommandType.FusionS);
        bots.Sort((x, y) => x.Bid - y.Bid);
        time++;
        executed += initialBotsCount;
    }

    private void UncheckedRun(Bot bot, Command command)
    {
        switch (command.Type)
        {
            case CommandType.Wait:
                return;
            case CommandType.Halt:
                for (int i = 0; i < Volume; i++)
                {
                    Assert.AreEqual(GetMatrix(i, fillBit), GetMatrix(i, modelBit));
                }
                Assert.AreEqual(bots.Count, 1);
                Assert.AreEqual(bot.Pos, Vector3Int.zero);
                Assert.IsFalse(harmonics);
                bots.Clear();
                return;
            case CommandType.Smove:
                bot.Move(command.Diff1);
                return;
            case CommandType.Lmove:
                bot.Move(command.Diff1);
                bot.Move(command.Diff2);
                return;
            case CommandType.Flip:
                harmonics = !harmonics;
                return;
            case CommandType.Fill:
                int fillPos = PosToIndex(bot.Pos + command.Diff1);
                Assert.IsTrue(GetMatrix(fillPos, modelBit));
                SetMatrix(fillPos, fillBit, true);
                return;
            case CommandType.Fission:
                bots.Add(bot.Fission(command.Diff1, command.Number));
                return;
            case CommandType.FusionP:
                Vector3Int secondaryPos = bot.Pos + command.Diff1;
                foreach (Bot other in bots)
                {
                    if (other.Pos == secondaryPos)
                    {
                        Assert.AreEqual(other.LastCommand.Type, CommandType.FusionS);
                        bot.Fusion(other);
                        return;
                    }
                }
                return;
            case CommandType.FusionS:
                Vector3Int primaryPos = bot.Pos + command.Diff1;
                foreach (Bot other in bots)
                {
                    if (other.Pos == primaryPos)
                    {
                        Assert.AreEqual(other.LastCommand.Type, CommandType.FusionP);
                        return;
                    }
                }
                return;
        }
    }

    public void Write(BinaryWriter writer)
    {
        foreach (var command in trace)
        {
            command.Write(writer);
        }
    }

    public void Write(TextWriter writer)
    {
        foreach (var command in trace)
        {
            command.Write(writer);
        }
    }

    public void ReadModel(BinaryReader reader)
    {
        energy = 0;
        harmonics = false;
        executed = 0;
        bots = new List<Bot>() { Bot.Init() };
        resolution = reader.ReadByte();
        Debug.Log("Resolution: " + resolution);
        matrix = new byte[resolution * resolution * resolution];
        for (int i = 0; i < matrix.Length / 8; ++i)
        {
            byte read = reader.ReadByte();
            for (int j = 0; j < 8; j++)
            {
                SetMatrix(i * 8 + j, modelBit, ((read >> j) & 1) != 0);
            }
        }
    }

    public Vector3Int IndexToPos(int index)
    {
        int x = index / (resolution * resolution);
        int remain = index - x * (resolution * resolution);
        int y = remain / resolution;
        int z = remain - y * resolution;
        return new Vector3Int(x, y, z);
    }

    public int PosToIndex(Vector3Int pos)
    {
        return pos.z + (pos.y + pos.x * resolution) * resolution;
    }

    private void SetMatrix(int index, int bit, bool value)
    {
        if (value)
        {
            matrix [index] |= (byte)(1 << bit);
        } else
        {
            matrix [index] &= (byte)(0xFF ^ (1 << bit));
        }
    }

    private bool GetMatrix(int index, int bit)
    {
        return ((matrix [index] >> bit) & 1) != 0;
    }

    public bool ShouldFill(int index)
    {
        return GetMatrix(index, modelBit);
    }

    public bool IsFilled(int index)
    {
        return GetMatrix(index, fillBit);
    }

    public void Clear()
    {
        energy = 0;
        harmonics = false;
        executed = 0;
        bots = new List<Bot>() { Bot.Init() };
        resolution = 1;
        matrix = new byte[1];
    }

    public void ClearCommand()
    {
        trace.Clear();
    }
}
