using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataControll : MonoBehaviour
{
    public void Start()
    {
        JSON player = new JSON();
        savePlayerData(player);

        JSON player2 = loadPlayerData();


    }

    public void savePlayerData(JSON player)
    {
        StreamWriter writer;

        string jsonstr = JsonUtility.ToJson(player);

        writer = new StreamWriter(Application.dataPath + "/savedata.json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }

    public JSON loadPlayerData()
    {
        string datastr = "";
        StreamReader reader;
        reader = new StreamReader(Application.dataPath + "/savedata.json");
        datastr = reader.ReadToEnd();
        reader.Close();

        return JsonUtility.FromJson<JSON>(datastr);
    }
}
