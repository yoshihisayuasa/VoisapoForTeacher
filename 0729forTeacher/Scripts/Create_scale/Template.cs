using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Template : MonoBehaviour
{
    //Dropdownを格納する変数
    [SerializeField] private TMP_Dropdown dropdown;
    //Cubeを格納する変数
    // [SerializeField] private InputField inputField;
    [SerializeField] Text inputField;
    [SerializeField] Button Clear;

    // オプションが変更されたときに実行するメソッド

    public static int i; //counter x軸
    public static int s;//counter y軸
    public static int k;//音符の数
    private TextMeshProUGUI[] textobject_textmesh = new TextMeshProUGUI[27];


    public static int[,] arr_fvtn = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 2, 1 }, { 4, 1 }, { 5, 1 }, { 7, 1 }, { 5, 1 }, { 4, 1 }, { 2, 1 }, { 0, 3 } };//初めの3つは和音
    public static int[,] arr_thrtn = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };
    public static int[,] arr_Oct = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };
    public static int[,] arr_OctHf = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 16, 1 }, { 19, 1 }, { 17, 1 }, { 14, 1 }, { 11, 1 }, { 7, 1 }, { 5, 1 }, { 2, 1 }, { 0, 3 } };
    public static int[,] arr_OctRpt = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };
    public static int[,] arr_OctDwn = new int[,] { { -5, 3 }, { -8, 3 }, { -12, 3 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 3 } };
    public static int[,] arr_OctRptDwn = new int[,] { { -5, 3 }, { -8, 3 }, { -12, 3 }, { 0, 1 }, { 0, 1 }, { 0, 1 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 3 } };
    public static int[,] arr_OctRptVib = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 12, 1 }, { 12, 1 }, { 12, 3 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };
    public static int[,] arr_BknApg = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 7, 1 }, { 4, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 1 }, { 7, 1 }, { 4, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };
    public static int[,] arr_Major = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, };
    public static int[,] arr_BkFth = new int[,] { { -5, 3 }, { -8, 3 }, { -12, 3 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 1 }, { -8, 1 }, { -5, 1 }, { 0, 1 }, { -5, 1 }, { -8, 1 }, { -12, 3 } }; //小久保先生に確認中
    public static int[,] arr_twct = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 }, { 4, 1 }, { 7, 1 }, { 12, 1 }, { 16, 1 }, { 19, 1 }, { 24, 1 }, { 19, 1 }, { 16, 1 }, { 12, 1 }, { 7, 1 }, { 4, 1 }, { 0, 3 } };//小久保先生に確認中
                                                                                                                                                                                                                         //  public static int[,] arr_sngl = new int[,] { { 7, 3 }, { 4, 3 }, { 0, 3 }, { 0, 1 } };


    void Start()
    {

        k = 0;
        i = 0;
        s = 0;
        for (int i = 0; i < 27; i++)
        {

            GameObject textobject = GameObject.Find("tx" + i);
            textobject_textmesh[i] = textobject.GetComponent<TextMeshProUGUI>();

        }
    }

    public void ChangeTemplate()
    {
        CreateScaleControll.SaveBtn.interactable = true;

        {
            if (dropdown.value == 0)
            {
                inputField.text = "";
            }

            else if (dropdown.value == 1)
            {
                txt_template(arr_fvtn);
                inputField.text = "5tone";
            }
            else if (dropdown.value == 2)
            {
                txt_template(arr_Oct);
                inputField.text = "1octave";
            }
            else if (dropdown.value == 3)
            {
                txt_template(arr_OctHf);
                inputField.text = "1.5octave";
            }
            else if (dropdown.value == 4)
            {
                txt_template(arr_OctRpt);
                inputField.text = "Octave Rpeat";
            }
            else if (dropdown.value == 5)
            {
                txt_template(arr_OctDwn);
                inputField.text = "[↓]Octave";
            }
            else if (dropdown.value == 6)
            {
                txt_template(arr_OctRptDwn);
                inputField.text = "[↓]Octave Rpeat";
            }
            else if (dropdown.value == 7)
            {
                txt_template(arr_OctRptVib);
                inputField.text = "[～]Octave Rpeat";
            }
            else if (dropdown.value == 8)
            {
                txt_template(arr_BknApg);
                inputField.text = "Broken Arpeggio";
            }
            else if (dropdown.value == 9)
            {
                txt_template(arr_BkFth);
                inputField.text = "Back&Forth";
            }
            else if (dropdown.value == 10)
            {
                txt_template(arr_twct);
                inputField.text = "2octave";
            }
            else if (dropdown.value == 11)
            {
                txt_template(arr_thrtn);
                inputField.text = "3tone";
            }



        }

        void txt_template(int[,] array)
        {
            Clear.onClick.Invoke();

            for (int i = 0; i < 27; i++)   //初期化
            {
                CreateScaleControll.int_clkdky[i, 0] = 0;
                CreateScaleControll.int_clkdky[i, 1] = 0;

            }



            CreateScaleControll.k = array.Length / 2;
            CreateScaleControll.i = (array.Length - 2) / 2;


            for (int i = 0; i < 3; i++)
            {
                textobject_textmesh[i].text = CreateScaleControll.pianokey88[array[i, 0] + 39];
            }



            int s = 3;
            int x = 3;

            while (true)
            {
                if (x > (array.Length / 2 - 1))
                {

                    break;
                }

                for (int m = 0; m < array[x, 1]; m++)
                {
                    if (m == 0)
                    {
                        textobject_textmesh[s].text = CreateScaleControll.pianokey88[array[x, 0] + 39];
                    }
                    if (m > 0)
                    {
                        textobject_textmesh[s].text = "-";
                        CreateScaleControll.arr[s].SetActive(true);
                    }
                    s = s + 1;
                }
                x = x + 1;
            }


            for (int i = 0; i < array.Length / 2; i++)
            {
                CreateScaleControll.int_clkdky[i, 0] = array[i, 0] + 39;
                CreateScaleControll.int_clkdky[i, 1] = array[i, 1];
            }



        }
    }
}