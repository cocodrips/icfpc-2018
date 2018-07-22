using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        using(var writer = new StreamWriter("Hoge"))
        {
            writer.WriteLine("Hoge");
        }
    }
}
