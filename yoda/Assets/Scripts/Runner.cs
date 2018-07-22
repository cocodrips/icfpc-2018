using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Runner : MonoBehaviour
{
    public bool play;
    public State state;
    public Object model;
    public GameObject filledPrefab;
    public GameObject botPrefab;
    public List<GameObject> filledObjects = new List<GameObject>();
    public List<GameObject> botObjects = new List<GameObject>();

    void Start()
    {
        if (filledPrefab)
        {
            filledPrefab.SetActive(false);
        }
        if (botPrefab)
        {
            botPrefab.SetActive(false);
        }
        ReadModel();
        ShowBots();
        AI ai = new AI(state.Resolution, state.ShouldFill);
        state.AddCommand(ai.Compute());
    }

    void Update()
    {
        if (play)
        {
            Step();
        }
    }

    [ContextMenu("Show model")]
    void ShowModel()
    {
        for (int i = 0; i < state.Volume; i++)
        {
            FillByIndex(i, state.ShouldFill(i));
        }
    }

    [ContextMenu("Show filled")]
    void ShowFilled()
    {
        for (int i = 0; i < state.Volume; i++)
        {
            FillByIndex(i, state.IsFilled(i));
        }
    }

    [ContextMenu("Add filled")]
    void AddFilled()
    {
        foreach (Bot bot in state.Bots)
        {
            if (bot.LastCommand.Type == CommandType.Fill)
            {
                FillByIndex(state.PosToIndex(bot.Pos + bot.LastCommand.Diff1), true);
            }
        }
    }

    void FillByIndex(int i, bool visible)
    {
        while (i >= filledObjects.Count)
        {
            filledObjects.Add(null);
        }
        if (visible)
        {
            if (!filledObjects[i])
            {
                filledObjects[i] = Instantiate<GameObject>(filledPrefab, transform);
            }
            filledObjects[i].transform.position = state.IndexToPos(i);
            filledObjects[i].SetActive(true);
        }
        else if (filledObjects[i])
        {
            filledObjects[i].SetActive(false);
        }
    }

    [ContextMenu("Show bots")]
    void ShowBots()
    {
        while (botObjects.Count < 21)
        {
            botObjects.Add(null);
        }
        foreach (GameObject botObject in botObjects)
        {
            if (botObject)
            {
                botObject.SetActive(false);
            }
        }
        foreach (Bot bot in state.Bots)
        {
            if (!botObjects[bot.Bid])
            {
                botObjects[bot.Bid] = Instantiate<GameObject>(botPrefab, transform);
                botObjects[bot.Bid].name = "Bid " + bot.Bid;
            }
            botObjects[bot.Bid].transform.position = bot.Pos;
            botObjects[bot.Bid].SetActive(true);
        }
    }

    [ContextMenu("Step")]
    void Step()
    {
        state.Step();
        ShowBots();
        AddFilled();
    }

    [ContextMenu("Clear")]
    void Clear()
    {
        state.Clear();
    }

#if UNITY_EDITOR
    [ContextMenu("Read model")]
    void ReadModel()
    {
        using (var br = new BinaryReader(File.OpenRead(UnityEditor.AssetDatabase.GetAssetPath(model))))
        {
            state.ReadModel(br);
        }
    }

    [ContextMenu("Write")]
    void Write()
    {
        using (var sw = new StreamWriter(Application.dataPath + "/Temp/yoda.ntt"))
        {
            state.Write(sw);
        }

        using (var bw = new BinaryWriter(File.OpenWrite(Application.dataPath + "/Temp/yoda.nbt")))
        {
            state.Write(bw);
        }
        UnityEditor.AssetDatabase.Refresh();
    }

    [ContextMenu("Compute all")]
    void RunAll()
    {
        try
        {
            string targetDir = Application.dataPath + "/Temp/" + System.DateTime.Now.ToString("dd-HH-mm-ss");
            print(targetDir);
            string[] problems = Directory.GetFiles(Application.dataPath + "/../../data/problemsL/");
            for (int i = 0; i < problems.Length; ++i)
            {
                string problem = problems[i];
                if (!problem.EndsWith("_tgt.mdl"))
                {
                    continue;
                }
                string problemName = problem.Substring(problem.Length - 13, 5);
                bool cancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar("Run AI", problemName, (float)i / problems.Length);
                if (cancel)
                {
                    break;
                }
                Directory.CreateDirectory(targetDir);
                using (var br = new BinaryReader(File.OpenRead(problem)))
                {
                    AI ai = new AI(br);
                    ai.Compute();
                    using (var bw = new BinaryWriter(File.OpenWrite(targetDir + "/" + problemName + ".nbt")))
                    {
                        ai.Write(bw);
                    }
                    using (var sw = new StreamWriter(targetDir + "/" + problemName + ".ntt"))
                    {
                        ai.Write(sw);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            UnityEditor.EditorUtility.ClearProgressBar();
        }
    }
#endif
}
