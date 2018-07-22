using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Bot
{
    [SerializeField] int bid;
    [SerializeField] Vector3Int pos;
    [SerializeField] List<int> seeds;
    [SerializeField] Command lastCommand;

    public int Bid { get { return bid; } }

    public Vector3Int Pos { get { return pos; } }

    public int NumSeeds { get { return seeds.Count; } }

    public Command LastCommand { get { return lastCommand; } set { lastCommand = value; } }

    private Bot(int bid, Vector3Int pos, List<int> seeds)
    {
        this.bid = bid;
        this.pos = pos;
        this.seeds = seeds;
        this.lastCommand = Command.None();
    }

    public static Bot Init()
    {
        var seeds = new List<int>();
        for (int i = 2; i <= 20; i++)
        {
            seeds.Add(i);
        }
        return new Bot(1, Vector3Int.zero, seeds);
    }

    public void Move(Vector3Int diff)
    {
        pos += diff;
    }

    public Bot Fission(Vector3Int diff, int number)
    {
        Assert.IsTrue(number < seeds.Count - 1);
        var parent = new List<int>(number);
        var child = new List<int>(seeds.Count - number - 1);
        for (int i = 1; i <= number; i++)
        {
            parent.Add(seeds[i]);
        }
        for (int i = number + 1; i < seeds.Count; i++)
        {
            child.Add(seeds[i]);
        }
        seeds = parent;
        return new Bot(seeds[0], pos + diff, child);
    }

    public void Fusion(Bot secondary)
    {
        seeds.Add(secondary.bid);
        seeds.AddRange(secondary.seeds);
        seeds.Sort();
    }
}
