using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using UnityEditor;




public class Save : MonoBehaviour
{
    public InputField inputField;
    public Button crear;
    [SerializeField] GameObject PopUp;

    //int lowestkey = 0;
    bool noduplication = true;
    int z = 0;
    //int id = 0;
    static string DataFileName = "savedata2";



    public void NameScale()
    {

        GameObject Panel_prefab = Resources.Load<GameObject>("RedSphere");
        GameObject Panel = Instantiate(Panel_prefab);


    }

    public void Save_Scl()
    {


        JSON SaveData = loadPlayerData();
        //一旦全部ロードしてる
        for (int i = 0; i < 27; i++)
        {

            ControlScale.arr_orig[ControlScale.scl_num, i, 0] = CreateScaleControll.int_clkdky[i, 0] - CreateScaleControll.int_clkdky[3, 0];
            ControlScale.arr_orig[ControlScale.scl_num, i, 1] = CreateScaleControll.int_clkdky[i, 1];
        }


        for (z = 0; z < 27; z++)
        {
            noduplication = true;
            for (int x = 0; x < ControlScale.scl_num; x++) //自分のポジションは検索しないのでscl_numでいい
            {
                if (ControlScale.scale_pos[x] == z || z == MoveDrag.PosDelBtn) //10番はゴミ箱があるため使用禁止
                {
                    noduplication = false;
                }
            }
            if (noduplication)
            {
                goto END;
            }
        }
    END:

        ControlScale.scale_pos[ControlScale.scl_num] = z;
        ControlScale.input_scale_name[ControlScale.scl_num] = inputField.GetComponent<InputField>().text;




        ControlScale.scl_num = ControlScale.scl_num + 1;

        JSON SaveData1 = new JSON();

        savearray(SaveData1, true);
        //saveScalenum(SaveData);

        crear.onClick.Invoke();


        SceneManager.LoadScene("SampleScene");


    }

    public void Home()
    {
        SceneManager.LoadScene("SampleScene");

    }
    public void SaveMelody()
    {
        int k = 0;
        for (k = 0; k < 27; k++)
        {
            if (CreateScaleControll.int_clkdky[k, 1] == 0)
            {
                break;
            }

        }

        if (k > 3) //和音の後の1音まで入力されていることを確認。和音の次の音を基準にシフトしているので、和音の次の音がない場合は差し引きできないのでシフトされなくなってしまう。
        {
            PopUp.SetActive(true);
        }

    }


    public static void savePlayerData(JSON SaveData)
    {
        StreamWriter writer;

        string jsonstr = JsonUtility.ToJson(SaveData);
        writer = new StreamWriter(Application.persistentDataPath + "/" + DataFileName + ".json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }



    public static JSON loadPlayerData()
    {
        string datastr = "";
        if (!(System.IO.File.Exists(Application.persistentDataPath + "/" + DataFileName + ".json")))
        {

            var data = Resources.Load<TextAsset>(DataFileName);
            File.AppendAllText(Application.persistentDataPath + "/" + DataFileName + ".json", data.ToString());

        }


        StreamReader reader;
        reader = new StreamReader(Application.persistentDataPath + "/" + DataFileName + ".json");
        datastr = reader.ReadToEnd();
        reader.Close();
        return JsonUtility.FromJson<JSON>(datastr);

    }

    public static void savearray(JSON SaveData, bool CreOrDelScale)
    {

        if (CreOrDelScale)
        {
            for (int i = 0; i < 27; i++)
            {
                //Note0 0番目のスケール　iがなくてショートしてる
                SaveData.ScaleNote0[i] = ControlScale.arr_orig[0, i, 0];
                SaveData.ScaleBeat0[i] = ControlScale.arr_orig[0, i, 1];
                SaveData.ScaleNote1[i] = ControlScale.arr_orig[1, i, 0];
                SaveData.ScaleBeat1[i] = ControlScale.arr_orig[1, i, 1];
                SaveData.ScaleNote2[i] = ControlScale.arr_orig[2, i, 0];
                SaveData.ScaleBeat2[i] = ControlScale.arr_orig[2, i, 1];
                SaveData.ScaleNote3[i] = ControlScale.arr_orig[3, i, 0];
                SaveData.ScaleBeat3[i] = ControlScale.arr_orig[3, i, 1];
                SaveData.ScaleNote4[i] = ControlScale.arr_orig[4, i, 0];
                SaveData.ScaleBeat4[i] = ControlScale.arr_orig[4, i, 1];
                SaveData.ScaleNote5[i] = ControlScale.arr_orig[5, i, 0];
                SaveData.ScaleBeat5[i] = ControlScale.arr_orig[5, i, 1];
                SaveData.ScaleNote6[i] = ControlScale.arr_orig[6, i, 0];
                SaveData.ScaleBeat6[i] = ControlScale.arr_orig[6, i, 1];
                SaveData.ScaleNote7[i] = ControlScale.arr_orig[7, i, 0];
                SaveData.ScaleBeat7[i] = ControlScale.arr_orig[7, i, 1];
                SaveData.ScaleNote8[i] = ControlScale.arr_orig[8, i, 0];
                SaveData.ScaleBeat8[i] = ControlScale.arr_orig[8, i, 1];
                SaveData.ScaleNote9[i] = ControlScale.arr_orig[9, i, 0];
                SaveData.ScaleBeat9[i] = ControlScale.arr_orig[9, i, 1];
                SaveData.ScaleNote10[i] = ControlScale.arr_orig[10, i, 0];
                SaveData.ScaleBeat10[i] = ControlScale.arr_orig[10, i, 1];
                SaveData.ScaleNote11[i] = ControlScale.arr_orig[11, i, 0];
                SaveData.ScaleBeat11[i] = ControlScale.arr_orig[11, i, 1];
                SaveData.ScaleNote12[i] = ControlScale.arr_orig[12, i, 0];
                SaveData.ScaleBeat12[i] = ControlScale.arr_orig[12, i, 1];
                SaveData.ScaleNote13[i] = ControlScale.arr_orig[13, i, 0];
                SaveData.ScaleBeat13[i] = ControlScale.arr_orig[13, i, 1];
                SaveData.ScaleNote14[i] = ControlScale.arr_orig[14, i, 0];
                SaveData.ScaleBeat14[i] = ControlScale.arr_orig[14, i, 1];
                SaveData.ScaleNote15[i] = ControlScale.arr_orig[15, i, 0];
                SaveData.ScaleBeat15[i] = ControlScale.arr_orig[15, i, 1];
                SaveData.ScaleNote16[i] = ControlScale.arr_orig[16, i, 0];
                SaveData.ScaleBeat16[i] = ControlScale.arr_orig[16, i, 1];
                SaveData.ScaleNote17[i] = ControlScale.arr_orig[17, i, 0];
                SaveData.ScaleBeat17[i] = ControlScale.arr_orig[17, i, 1];
                SaveData.ScaleNote18[i] = ControlScale.arr_orig[18, i, 0];
                SaveData.ScaleBeat18[i] = ControlScale.arr_orig[18, i, 1];
                SaveData.ScaleNote19[i] = ControlScale.arr_orig[19, i, 0];
                SaveData.ScaleBeat19[i] = ControlScale.arr_orig[19, i, 1];
                SaveData.ScaleNote20[i] = ControlScale.arr_orig[20, i, 0];
                SaveData.ScaleBeat20[i] = ControlScale.arr_orig[20, i, 1];
                SaveData.ScaleNote21[i] = ControlScale.arr_orig[21, i, 0];
                SaveData.ScaleBeat21[i] = ControlScale.arr_orig[21, i, 1];
                SaveData.ScaleNote22[i] = ControlScale.arr_orig[22, i, 0];
                SaveData.ScaleBeat22[i] = ControlScale.arr_orig[22, i, 1];
                SaveData.ScaleNote23[i] = ControlScale.arr_orig[23, i, 0];
                SaveData.ScaleBeat23[i] = ControlScale.arr_orig[23, i, 1];
                SaveData.ScaleNote24[i] = ControlScale.arr_orig[24, i, 0];
                SaveData.ScaleBeat24[i] = ControlScale.arr_orig[24, i, 1];
                SaveData.ScaleNote25[i] = ControlScale.arr_orig[25, i, 0];
                SaveData.ScaleBeat25[i] = ControlScale.arr_orig[25, i, 1];
                SaveData.ScaleNote26[i] = ControlScale.arr_orig[26, i, 0];
                SaveData.ScaleBeat26[i] = ControlScale.arr_orig[26, i, 1];


                SaveData.ScaleName[i] = ControlScale.input_scale_name[i];
            }

            SaveData.ScaleNum = ControlScale.scl_num;

        }

        for (int i = 0; i < 27; i++)
        {
            SaveData.ScalePos[i] = ControlScale.scale_pos[i];
        }

        savePlayerData(SaveData);
    }


}