//次やること：Up もしくはダウンしたらキーの色を変更
//クリック時もキーの色を変更
//キーに色を付与


using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


//登録するスケールの音数は20を限度と設定
public class CreateScaleControll : MonoBehaviourPunCallbacks
{
    AudioSource sounds;


    //10*10のint型２次元配列を定義
    //
    private GameObject clickedGameObject;

    private string[] str_clkdky = new string[27];
    public static int[,] int_clkdky = new int[27, 2];
    public static int i; //counter x軸
    public static int s;//counter y軸
    public static int k;//音符の数
    public static string[] pianokey88 = new string[]  { "A0", "A#0","B0",
                                               "C1","C#1","D1","D#1","E1","F1","F#1","G1","G#1", "A1", "A#1","B1",
                                               "C2","C#2","D2","D#2","E2","F2","F#2","G2","G#2", "A2", "A#2","B2",
                                               "C3","C#3","D3","D#3","E3","F3","F#3","G3","G#3", "A3", "A#3","B3",
                                               "C4","C#4","D4","D#4","E4","F4","F#4","G4","G#4", "A4", "A#4","B4",
                                               "C5","C#5","D5","D#5","E5","F5","F#5","G5","G#5", "A5", "A#5","B5",
                                               "C6","C#6","D6","D#6","E6","F6","F#6","G6","G#6", "A6", "A#6","B6",
                                               "C7","C#7","D7","D#7","E7","F7","F#7","G7","G#7", "A7", "A#7","B7",
                                               "C8" };
    AudioClip[] clip = new AudioClip[88];
    private TextMeshProUGUI[] textobject_textmesh = new TextMeshProUGUI[28];

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Text Label;
    public static Button SaveBtn;
    public static SpriteRenderer[] component_cash = new SpriteRenderer[88];
    GameObject[] object2 = new GameObject[88];

    int index_onmouse = 0;
    public static GameObject[] arr = new GameObject[28];

    string clkdky_cash;
    string clkdky_cash_1;

    void Start()
    {
        SaveBtn = GameObject.Find("Save").GetComponent<Button>();
        SaveBtn.interactable = false;

        k = 0;
        i = 0;


        //配列初期化
        for (k = 0; k < 27; k++)
        {
            int_clkdky[k, 0] = 0;
            int_clkdky[k, 1] = 0;
            str_clkdky[k] = null;
        }


        //カメラ情報を取得
        sounds = GetComponent<AudioSource>();


        for (int i = 0; i < 27; i++)
        {
            GameObject textobject = GameObject.Find("tx" + i);
            textobject_textmesh[i] = textobject.GetComponent<TextMeshProUGUI>();
            textobject_textmesh[i].color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
        }
        for (int i = 0; i < 88; i++)
        {
            object2[i] = GameObject.Find(i.ToString());
            component_cash[i] = object2[i].GetComponent<SpriteRenderer>();
            clip[i] = Resources.Load("ogg/" + i, typeof(AudioClip)) as AudioClip;
        }

        for (int i = 3; i < 27; i++)
        {
            arr[i] = GameObject.Find("ar" + i);
            arr[i].SetActive(false);
        }


    }

    void Update()
    {

        //マウスがクリックされたとき
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2d = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction);
            if (EventSystem.current.IsPointerOverGameObject()) return; //レイキャスト透過防止

            if (hit2d)
            {

                if (hit2d.transform.gameObject.CompareTag("key"))
                {

                    if (clickedGameObject != null)
                    {

                        if (pianokey88[int.Parse(clickedGameObject.name)].Contains("#"))
                        {
                            component_cash[index_onmouse].color = Color.black;
                        }
                        else
                        {
                            component_cash[index_onmouse].color = Color.white;
                        }
                    }


                    for (k = 0; k < 27; k++)
                    {
                        if (int_clkdky[k, 1] == 0)
                        {
                            break;
                        }

                    }

                    for (i = 0; i < 27; i++)
                    {
                        if (textobject_textmesh[i].text == "")
                        {
                            break;
                        }

                    }
                    if (i < 27) //入力テキスト上限
                    {
                        clickedGameObject = hit2d.transform.gameObject;
                        index_onmouse = int.Parse(clickedGameObject.name);

                        sounds.Stop();
                        sounds.PlayOneShot(clip[index_onmouse], volcontroll.newSliderValue);

                        component_cash[index_onmouse].color = Color.red;
                        str_clkdky[k] = pianokey88[index_onmouse];

                        int_clkdky[k, 0] = index_onmouse;
                        int_clkdky[k, 1] = 1;


                        if (k > 2)
                        {
                            if (SaveBtn.interactable == false)
                            {
                                SaveBtn.interactable = true;
                            }
                        }
                        else
                        {
                            if (SaveBtn.interactable == true)
                            {
                                SaveBtn.interactable = false;
                            }
                        }


                        if (k == 1)
                        {

                            if (int_clkdky[1, 0] > int_clkdky[0, 0])
                            {
                                clkdky_cash = str_clkdky[0];
                                str_clkdky[0] = str_clkdky[1];
                                str_clkdky[1] = clkdky_cash;

                                textobject_textmesh[0].text = str_clkdky[0];
                                textobject_textmesh[1].text = str_clkdky[1];
                            }
                        }

                        if (k == 2)
                        {

                            if (int_clkdky[2, 0] > int_clkdky[1, 0])
                            {
                                if (int_clkdky[2, 0] > int_clkdky[0, 0])
                                {
                                    clkdky_cash = str_clkdky[0];
                                    clkdky_cash_1 = str_clkdky[1];
                                    str_clkdky[1] = clkdky_cash;
                                    str_clkdky[0] = str_clkdky[2];
                                    str_clkdky[2] = clkdky_cash_1;
                                }
                                else
                                {

                                    clkdky_cash = str_clkdky[1];
                                    str_clkdky[1] = str_clkdky[2];
                                    str_clkdky[2] = clkdky_cash;


                                }

                            }
                            textobject_textmesh[0].text = str_clkdky[0];
                            textobject_textmesh[1].text = str_clkdky[1];
                            textobject_textmesh[2].text = str_clkdky[2];


                        }


                        textobject_textmesh[i].text = str_clkdky[k];


                        if (i < 3)
                        {
                            int_clkdky[k, 1] = 3;
                        }


                    }


                }

            }
        }
    }



    public void Del()
    {


        ///標示の処理///////////////////////////////////////////////////////////
        for (k = 0; k < 27; k++)
        {
            if (int_clkdky[k, 1] == 0)
            {
                break;
            }

        }

        for (i = 0; i < 27; i++)
        {
            if (textobject_textmesh[i].text == "")//次が空白だったところでとまる。
            {
                break;
            }

        }
        k = k - 1;
        i = i - 1;



        if (i > -1)
        {
            if (textobject_textmesh[i] != null)
            {
                textobject_textmesh[i].text = "";
            }

            if (arr[i] != null)
            {
                arr[i].SetActive(false);
            }
            //表示の処理完了//////////////////////////////////////////////////////////////////

            //配列の処理//////////////////////////
            if (int_clkdky[k, 1] == 1) //kが1拍の場合は配列から音が消される
            {
                int_clkdky[k, 0] = 0;
                int_clkdky[k, 1] = 0;

            }
            else if (k < 3) //和音の場合は音をそのまま消す
            {
                int_clkdky[k, 1] = 0;
            }
            else
            {
                int_clkdky[k, 1] = int_clkdky[k, 1] - 1;
            }

            if (k < 4)
            {
                if (SaveBtn.interactable == true)
                {
                    SaveBtn.interactable = false;
                }
            }

        }

    }


    public void addbeat()
    {

        for (k = 0; k < 27; k++)
        {
            if (int_clkdky[k, 1] == 0)
            {
                break;
            }

        }

        for (i = 0; i < 27; i++)
        {
            if (textobject_textmesh[i].text == "")
            {
                break;
            }

        }
        if (i < 27) //入力テキスト上限
        {
            if (k > 3)
            {
                int_clkdky[k - 1, 1] = int_clkdky[k - 1, 1] + 1;
                str_clkdky[i] = "-";
                textobject_textmesh[i].text = "-";
                arr[i].SetActive(true);
            }
        }
    }

    public void crear()
    {
        SaveBtn.interactable = false;

        dropdown.value = 0;
        i = 0;
        k = 0;
        s = 0;

        sounds.Stop();

        for (int i = 0; i < 27; i++)
        {
            textobject_textmesh[i].text = "";

        }

        for (int i = 3; i < 27; i++)
        {
            arr[i].SetActive(false);
        }
        for (int m = 0; m < 27; m++)
        {
            int_clkdky[m, 0] = 0;
            int_clkdky[m, 1] = 0;
        }

        dropdown.value = 0;
        // inputField.GetComponent<InputField>().text = "";
        Label.text = "メロディー 読込";


    }

}


