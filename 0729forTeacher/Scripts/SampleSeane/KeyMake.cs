using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyMake : MonoBehaviour
{

    //https://qiita.com/mczkzk/items/61f37dbd9444699c0d35
    private Transform MyTr;
    private int i = 0;
    // Start is called before the first frame update
    void Awake()
    {

        MyTr = transform;
        MakeKeys();

    }

    void MakeKeys()
    {


        float KeySpace = 0.021f;
        float bairitsu;
        bairitsu = 1.0f;

        KeySpace = KeySpace * bairitsu;

        string[] pianokey88 = new string[]  { "A0", "A#0","B0",
                                               "C1","C#1","D1","D#1","E1","F1","F#1","G1","G#1", "A1", "A#1","B1",
                                               "C2","C#2","D2","D#2","E2","F2","F#2","G2","G#2", "A2", "A#2","B2",
                                               "C3","C#3","D3","D#3","E3","F3","F#3","G3","G#3", "A3", "A#3","B3",
                                               "C4","C#4","D4","D#4","E4","F4","F#4","G4","G#4", "A4", "A#4","B4",
                                               "C5","C#5","D5","D#5","E5","F5","F#5","G5","G#5", "A5", "A#5","B5",
                                               "C6","C#6","D6","D#6","E6","F6","F#6","G6","G#6", "A6", "A#6","B6",
                                               "C7","C#7","D7","D#7","E7","F7","F#7","G7","G#7", "A7", "A#7","B7",
                                               "C8" };


        foreach (string s in pianokey88)
        {

            float Xloc = i * KeySpace;
            if (s.Contains("#"))
            {
                Xloc -= KeySpace / 2; // 真ん中に

                GameObject theBKey = Instantiate(Resources.Load("Materials/Square_B") as GameObject, MyTr.position + new Vector3(Xloc, 0.02f * bairitsu), Quaternion.identity);
                theBKey.name = "key" + s;
                theBKey.tag = theBKey.name;
                theBKey.transform.localScale = new Vector3(0.015f, 0.06f, 0);
                theBKey.transform.parent = MyTr;

            }
            else
            {
                GameObject theKey = Instantiate(Resources.Load("Materials/Square_W") as GameObject, MyTr.position + new Vector3(Xloc, 0), Quaternion.identity);
                theKey.name = "key" + s;
                theKey.tag = theKey.name;
                theKey.transform.localScale = new Vector3(0.02f, 0.1f, 0);
                theKey.transform.parent = MyTr;

                i = i + 1;
            }

        }


    }
}

