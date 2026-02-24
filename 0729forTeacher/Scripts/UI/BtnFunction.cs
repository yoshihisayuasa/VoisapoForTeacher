using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;
using UnityEngine.EventSystems;
//ショートカット入力できるものまとめ
public class BtnFunction : MonoBehaviour
{

    //スケール選択
    private static int befid = 99;

    private int id = 999;
    private static Image[] Btn_Image = new Image[27];
    int array_scale_max = 0;
    int array_scale_min = 0;
    public static bool MajorCodeSelected = false;


    public void click()
    {
        GameController.ScaleSelect = true; //スケールが変更したのを感知。スケール再選択時、最低音、最高音、クリックした鍵盤の着色を再実施


        if (this.gameObject.name == "SclBtn_1")
        {
            MajorCodeSelected = true;
        }
        else
        {
            MajorCodeSelected = false;
        }






        if (befid == 99 || befid != id)
        {
            id = Int32.Parse(this.gameObject.name.Remove(0, 7));

            if (Btn_Image[id] == null)
            {
                for (int i = 0; i < ControlScale.scl_num + 1; i++)
                {
                    GameObject Btn = GameObject.Find("SclBtn_" + i);
                    if (Btn != null)
                    {
                        Btn_Image[i] = Btn.GetComponent<Image>();
                    }
                }
            }

            if (befid != 99)
            {
                if (Btn_Image[befid] == null)
                {
                    for (int i = 0; i < ControlScale.scl_num; i++)
                    {
                        GameObject Btn = GameObject.Find("SclBtn_" + i);
                        if (Btn != null)
                        {
                            Btn_Image[i] = Btn.GetComponent<Image>();
                        }
                    }
                }
                if (Btn_Image[befid] != null)
                {
                    Btn_Image[befid].color = Color.white;
                }
            }
            if (Btn_Image[id] != null)
            {
                Btn_Image[id].color = Color.red;
            }
            befid = id;             //今回クリックしたボタンのIDを渡す 色をかえる

        }

        orig();

    }




    public void orig()
    {
        for (int m = 0; m < 27; m++) //配列の初期化
        {
            ControlScale.arr_scale[m, 0] = 0;
            ControlScale.arr_scale[m, 1] = 0;
        }

        int s = Int32.Parse(this.gameObject.name.Remove(0, 7));

        for (int r = 0; r < 27; r++)
        {
            ControlScale.arr_scale[r, 0] = ControlScale.arr_orig[s, r, 0];
            ControlScale.arr_scale[r, 1] = ControlScale.arr_orig[s, r, 1];
            max_min(ControlScale.arr_scale);

        }
    }



    void max_min(int[,] arr_scale)
    {


        int arr_scale_Length = arr_scale.Length / 2;



        for (int i = 0; i < arr_scale_Length; i++)
        {
            if (arr_scale[i, 1] == 0)
            {
                break;
            }

            if (array_scale_max < arr_scale[i, 0])
            {
                array_scale_max = arr_scale[i, 0];
            }
            if (array_scale_min > arr_scale[i, 0])
            {
                array_scale_min = arr_scale[i, 0];
            }

        }

        if (MajorCodeSelected)
        {
            array_scale_max = 12;
            array_scale_min = 0;
        }


        GameController.arr_scale_Length = arr_scale_Length;
        GameController.array_scale_max = array_scale_max;
        GameController.array_scale_min = array_scale_min;

    }





}










