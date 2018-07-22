using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Int3Type = UnityEngine.Vector3Int;

public class AI
{
    public const int ShouldFillId = -1;
    public const int VoidId = 0;
    public const int GroundId = 1;

    static readonly Int3Type[] neighbor = new Int3Type[]
    {
        int3(-1, 0, 0),
        int3(+1, 0, 0),
        int3(0, -1, 0),
        int3(0, +1, 0),
        int3(0, 0, -1),
        int3(0, 0, +1),
    };

    List<int> idToGroup = new List<int>() { 0, 1 };
    int groundedGroupIndex = 1;
    List<Command> commands = new List<Command>();
    int resolution;
    bool high;
    int[] state;

    public AI(int resolution, Func<int, bool> model)
    {
        this.resolution = resolution;
        this.state = new int[resolution * resolution * resolution];
        for (int i = 0; i < state.Length; i++)
        {
            if (model(i))
            {
                this.state[i] = ShouldFillId;
            }
        }
    }

    public AI(BinaryReader reader)
    {
        this.resolution = reader.ReadByte();
        this.state = new int[resolution * resolution * resolution];
        for (int i = 0; i < state.Length / 8; ++i)
        {
            byte read = reader.ReadByte();
            for (int j = 0; j < 8; j++)
            {
                if (((read >> j) & 1) != 0)
                {
                    this.state[i * 8 + j] = ShouldFillId;
                }
            }
        }
    }

    public List<Command> Compute()
    {
        this.commands.Clear();
        Int3Type botPos = int3(0, 0, 0);
        bool zDir = true;
        bool xDir = true;
        var mayFill = new List<Int3Type>();
        var toFill = new List<Int3Type>();
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x3 = 0; x3 < (resolution + 2) / 3; x3++)
            {
                for (int zi = 1; zi < resolution; zi++)
                {
                    int z = zDir ? zi : resolution - 1 - zi;
                    int x = xDir ? x3 * 3 + 1 : resolution - 1 - x3 * 3 - 1;

                    // Bot will move to here if needed
                    var botTarget = int3(x, y + 1, z);

                    toFill.Clear();
                    mayFill.Clear();
                    mayFill.Add(int3(x, y, z - 1)); // bottom prev
                    mayFill.Add(int3(x - 1, y, z)); // bottom left
                    mayFill.Add(int3(x + 1, y, z)); // bottom right

                    // Positions above should be filled now
                    int fillNowIndex = mayFill.Count;

                    mayFill.Add(int3(x, y, z));

                    bool fillNow = false;
                    for (int i = 0; i < mayFill.Count; ++i)
                    {
                        if (shouldFill(mayFill[i]))
                        {
                            fillNow |= i < fillNowIndex;
                            toFill.Add(mayFill[i]);
                        }
                    }
                    if (fillNow)
                    {
                        move(botTarget - botPos);
                        botPos = botTarget;
                        int loopDetection = 0;
                        int i = -1;
                        while (toFill.Count > 0)
                        {
                            i = (i + 1) % toFill.Count;
                            if (loopDetection >= toFill.Count)
                            {
                                flipFillMark(botPos, toFill[i]);
                                toFill.RemoveAt(i);
                            }
                            else if (willGround(toFill[i]))
                            {
                                flipFillMark(botPos, toFill[i]);
                                toFill.RemoveAt(i);
                                loopDetection = 0;
                            }
                            loopDetection++;
                        }
                        flipIfPossible();
                    }
                }
                zDir = !zDir;
            }
            xDir = !xDir;
        }
        if (high)
        {
            flip();
        }
        move(int3(-botPos.x, 0, -botPos.z));
        move(int3(0, -botPos.y, 0));
        halt();
        return commands;
    }

    // Psudo command
    void move(Int3Type diff)
    {
        while (diff.y != 0)
        {
            Int3Type lld = int3(0, longestLLD(diff.y), 0);
            diff -= lld;
            smove(lld);
        }
        while (Math.Abs(diff.x) > 5)
        {
            Int3Type lld = int3(longestLLD(diff.x), 0, 0);
            diff -= lld;
            smove(lld);
        }
        while (Math.Abs(diff.z) > 5)
        {
            Int3Type lld = int3(0, 0, longestLLD(diff.z));
            diff -= lld;
            smove(lld);
        }
        if (diff.x != 0 && diff.z != 0)
        {
            lmove(int3(diff.x, 0, 0), int3(0, 0, diff.z));
        }
        else if (diff.x != 0)
        {
            smove(int3(diff.x, 0, 0));
        }
        else if (diff.z != 0)
        {
            smove(int3(0, 0, diff.z));
        }
    }

    int longestLLD(int diff)
    {
        return Math.Sign(diff) * Math.Min(15, Math.Abs(diff));
    }

    // Psudo command
    void flipFillMark(Int3Type botPos, Int3Type fillPos)
    {
        markFill(fillPos);
        if (!high && !isGround(fillPos))
        {
            flip();
        }
        fill(fillPos - botPos);
    }

    static Int3Type int3(int x, int y, int z)
    {
        return new Int3Type(x, y, z);
    }

    void addCommand(Command command)
    {
        commands.Add(command);
    }

    void halt()
    {
        addCommand(Command.Halt());
    }

    void flip()
    {
        high = !high;
        addCommand(Command.Flip());
    }

    void smove(Int3Type diff)
    {
        addCommand(Command.Smove(diff));
    }

    void lmove(Int3Type diff1, Int3Type diff2)
    {
        addCommand(Command.Lmove(diff1, diff2));
    }

    void fill(Int3Type diff)
    {
        addCommand(Command.Fill(diff));
    }

    public Int3Type indexToPos(int index)
    {
        int x = index / (resolution * resolution);
        int remain = index - x * (resolution * resolution);
        int y = remain / resolution;
        int z = remain - y * resolution;
        return int3(x, y, z);
    }

    public int posToIndex(Int3Type pos)
    {
        return pos.z + (pos.y + pos.x * resolution) * resolution;
    }

    bool isValid(Int3Type pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0
        && pos.x < resolution && pos.y < resolution && pos.z < resolution;
    }

    // Returns true if it should be filled and not marked yet.
    bool shouldFill(Int3Type pos)
    {
        return isValid(pos) && state[posToIndex(pos)] == ShouldFillId;
    }

    void flipIfPossible()
    {
        if (!high)
        {
            return;
        }
        while (unionFind(groundedGroupIndex) == GroundId)
        {
            if (groundedGroupIndex < idToGroup.Count - 1)
            {
                groundedGroupIndex++;
            }
            else
            {
                flip();
                return;
            }
        }
    }

    bool isFilled(Int3Type pos)
    {
        return isValid(pos) && state[posToIndex(pos)] >= GroundId;
    }

    void markFill(Int3Type pos)
    {
        if (!isValid(pos))
        {
            // TODO: Error.
            return;
        }
        if (pos.y == 0)
        {
            state[posToIndex(pos)] = GroundId;
            return;
        }
        int minNonZero = idToGroup.Count;
        for (int i = 0; i < neighbor.Length; i++)
        {
            Int3Type x = pos + neighbor[i];
            if (!isValid(x))
            {
                continue;
            }
            int id = unionFind(state[posToIndex(x)]);
            if (id <= 0)
            {
                continue;
            }
            minNonZero = Math.Min(minNonZero, id);
        }
        if (minNonZero == idToGroup.Count)
        {
            idToGroup.Add(minNonZero);
        }
        else
        {
            for (int i = 0; i < neighbor.Length; i++)
            {
                int id = unionFind(pos + neighbor[i]);
                if (id > 0 && idToGroup[id] > 0)
                {
                    idToGroup[id] = Math.Min(idToGroup[id], minNonZero);
                }
            }
        }
        state[posToIndex(pos)] = minNonZero;
    }

    bool willGround(Int3Type pos)
    {
        if (!isValid(pos))
        {
            return false;
        }
        if (pos.y == 0)
        {
            return true;
        }
        for (int i = 0; i < neighbor.Length; i++)
        {
            if (isGround(pos + neighbor[i]))
            {
                return true;
            }
        }
        return false;
    }

    int unionFind(int id)
    {
        if (id < 0 || idToGroup[id] == id)
        {
            return id;
        }
        else
        {
            int newId = unionFind(idToGroup[id]);
            idToGroup[id] = newId;
            return newId;
        }
    }

    int unionFind(Int3Type pos)
    {
        return isValid(pos) ? unionFind(state[posToIndex(pos)]) : 0;
    }

    bool isGround(Int3Type pos)
    {
        return unionFind(pos) == GroundId;
    }

    public void Write(BinaryWriter writer)
    {
        foreach (var command in commands)
        {
            command.Write(writer);
        }
    }

    public void Write(TextWriter writer)
    {
        foreach (var command in commands)
        {
            command.Write(writer);
        }
    }
}
