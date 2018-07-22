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
                this.state [i] = ShouldFillId;
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
                    this.state [i * 8 + j] = ShouldFillId;
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
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x3 = 0; x3 < (resolution + 2) / 3; x3++)
            {
                for (int zi = 1; zi < resolution; zi++)
                {
                    int z = zDir ? zi : resolution - 1 - zi;
                    int x = xDir ? x3 * 3 + 1 : resolution - 1 - x3 * 3 - 1;
                    int xd = xDir ? 1 : -1;
                    var botTarget = int3(x, y + 1, z);
                    var prev = int3(x, y, z - 1);
                    var left = int3(x - xd, y, z);
                    var center = int3(x + 0, y, z);
                    var right = int3(x + xd, y, z);
                    bool needPrev = shouldFill(prev);
                    bool needLeft = shouldFill(left);
                    bool needCenter = shouldFill(center);
                    bool needRight = shouldFill(right);
                    if (needPrev || needLeft || needRight)
                    {
                        move(botTarget - botPos);
                        botPos = botTarget;
                        // TODO: Reorder and flip redunduncy
                        if (needPrev)
                        {
                            flipFillMark(botPos, prev);
                        }
                        if (needCenter)
                        {
                            flipFillMark(botPos, center);
                        }
                        if (needLeft)
                        {
                            flipFillMark(botPos, left);
                        }
                        if (needRight)
                        {
                            flipFillMark(botPos, right);
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
            int d = Math.Sign(diff.y) * Math.Min(15, Math.Abs(diff.y));
            diff.y -= d;
            smove(int3(0, d, 0));
        }
        while (Math.Abs(diff.x) > 5)
        {
            int d = Math.Sign(diff.x) * Math.Min(15, Math.Abs(diff.x));
            diff.x -= d;
            smove(int3(d, 0, 0));
        }
        while (Math.Abs(diff.z) > 5)
        {
            int d = Math.Sign(diff.z) * Math.Min(15, Math.Abs(diff.z));
            diff.z -= d;
            smove(int3(0, 0, d));
        }
        if (diff.x != 0 && diff.z != 0)
        {
            lmove(int3(diff.x, 0, 0), int3(0, 0, diff.z));
        } else if (diff.x != 0)
        {
            smove(int3(diff.x, 0, 0));
        } else if (diff.z != 0)
        {
            smove(int3(0, 0, diff.z));
        }
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
        return isValid(pos) && state [posToIndex(pos)] == ShouldFillId;
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
            } else
            {
                flip();
                return;
            }
        }
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
            state [posToIndex(pos)] = GroundId;
            return;
        }
        int minNonZero = idToGroup.Count;
        for (int i = 0; i < neighbor.Length; i++)
        {
            Int3Type x = pos + neighbor [i];
            if (!isValid(x))
            {
                continue;
            }
            int id = unionFind(state [posToIndex(x)]);
            if (id <= 0)
            {
                continue;
            }
            minNonZero = Math.Min(minNonZero, id);
        }
        if (minNonZero == idToGroup.Count)
        {
            idToGroup.Add(minNonZero);
        } else
        {
            for (int i = 0; i < neighbor.Length; i++)
            {
                int id = unionFind(pos + neighbor [i]);
                if (id > 0 && idToGroup [id] > 0)
                {
                    idToGroup [id] = Math.Min(idToGroup [id], minNonZero);
                }
            }
        }
        state [posToIndex(pos)] = minNonZero;
    }

    int unionFind(int id)
    {
        if (id < 0 || idToGroup [id] == id)
        {
            return id;
        } else
        {
            int newId = unionFind(idToGroup [id]);
            idToGroup [id] = newId;
            return newId;
        }
    }

    int unionFind(Int3Type pos)
    {
        return isValid(pos) ? unionFind(state [posToIndex(pos)]) : 0;
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
