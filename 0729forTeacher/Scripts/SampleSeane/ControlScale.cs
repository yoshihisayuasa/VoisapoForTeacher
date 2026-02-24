using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControlScale : MonoBehaviour
{
    public static int[,] arr_scale = new int[27, 2];
    public static int[,,] arr_orig = new int[27, 27, 2]; //0 scalename 1,key,2beat  [スケール数、音数,]
                                                         // public static int[,] arr_orig_cash = new int[27, 2];
    public static string[] input_scale_name = new string[27];
    public static int[] scale_pos = new int[27]; //列、配列名　数字、ポジション番号
    public static int scl_num;//初期値はデフォルトの数

    public static int KeyNum = 27; //array_origiの第二要素数


    public void Start()
    {

        for (int i = 0; i < 27; i++)
        {
            for (int j = 0; j < 27; j++)
            {
                arr_orig[i, j, 0] = 0;
                arr_orig[i, j, 1] = 0;
            }
        }
        JSON SaveData = Save.loadPlayerData();
        scl_num = SaveData.ScaleNum;

        for (int i = 0; i < 27; i++)
        {
            if (i < SaveData.ScaleNote0.Length)
            {
                arr_orig[0, i, 0] = SaveData.ScaleNote0[i];
            }
            if (i < SaveData.ScaleNote1.Length)
            {
                arr_orig[1, i, 0] = SaveData.ScaleNote1[i];
            }
            if (i < SaveData.ScaleNote2.Length)
            {
                arr_orig[2, i, 0] = SaveData.ScaleNote2[i];
            }
            if (i < SaveData.ScaleNote3.Length)
            {
                arr_orig[3, i, 0] = SaveData.ScaleNote3[i];
            }
            if (i < SaveData.ScaleNote4.Length)
            {
                arr_orig[4, i, 0] = SaveData.ScaleNote4[i];
            }
            if (i < SaveData.ScaleNote5.Length)
            {
                arr_orig[5, i, 0] = SaveData.ScaleNote5[i];

            }
            if (i < SaveData.ScaleNote6.Length)
            {
                arr_orig[6, i, 0] = SaveData.ScaleNote6[i];
            }
            if (i < SaveData.ScaleNote7.Length)
            {
                arr_orig[7, i, 0] = SaveData.ScaleNote7[i];
            }
            if (i < SaveData.ScaleNote8.Length)
            {
                arr_orig[8, i, 0] = SaveData.ScaleNote8[i];
            }
            if (i < SaveData.ScaleNote9.Length)
            {
                arr_orig[9, i, 0] = SaveData.ScaleNote9[i];
            }
            if (i < SaveData.ScaleNote10.Length)
            {
                arr_orig[10, i, 0] = SaveData.ScaleNote10[i];
            }
            if (i < SaveData.ScaleNote11.Length)
            {
                arr_orig[11, i, 0] = SaveData.ScaleNote11[i];
            }
            if (i < SaveData.ScaleNote12.Length)
            {
                arr_orig[12, i, 0] = SaveData.ScaleNote12[i];
            }
            if (i < SaveData.ScaleNote13.Length)
            {
                arr_orig[13, i, 0] = SaveData.ScaleNote13[i];
            }
            if (i < SaveData.ScaleNote14.Length)
            {
                arr_orig[14, i, 0] = SaveData.ScaleNote14[i];
            }
            if (i < SaveData.ScaleNote15.Length)
            {
                arr_orig[15, i, 0] = SaveData.ScaleNote15[i];
            }
            if (i < SaveData.ScaleNote16.Length)
            {
                arr_orig[16, i, 0] = SaveData.ScaleNote16[i];
            }
            if (i < SaveData.ScaleNote17.Length)
            {
                arr_orig[17, i, 0] = SaveData.ScaleNote17[i];
            }
            if (i < SaveData.ScaleNote18.Length)
            {
                arr_orig[18, i, 0] = SaveData.ScaleNote18[i];
            }
            if (i < SaveData.ScaleNote19.Length)
            {
                arr_orig[19, i, 0] = SaveData.ScaleNote19[i];
            }
            if (i < SaveData.ScaleNote20.Length)
            {
                arr_orig[20, i, 0] = SaveData.ScaleNote20[i];
            }
            if (i < SaveData.ScaleNote21.Length)
            {
                arr_orig[21, i, 0] = SaveData.ScaleNote21[i];
            }
            if (i < SaveData.ScaleNote22.Length)
            {
                arr_orig[22, i, 0] = SaveData.ScaleNote22[i];
            }
            if (i < SaveData.ScaleNote23.Length)
            {
                arr_orig[23, i, 0] = SaveData.ScaleNote23[i];
            }
            if (i < SaveData.ScaleNote24.Length)
            {
                arr_orig[24, i, 0] = SaveData.ScaleNote24[i];
            }
            if (i < SaveData.ScaleNote25.Length)
            {
                arr_orig[25, i, 0] = SaveData.ScaleNote25[i];
            }
            if (i < SaveData.ScaleNote26.Length)
            {
                arr_orig[26, i, 0] = SaveData.ScaleNote26[i];
            }



            if (i < SaveData.ScaleBeat0.Length)
            {
                arr_orig[0, i, 1] = SaveData.ScaleBeat0[i];
            }
            if (i < SaveData.ScaleBeat1.Length)
            {
                arr_orig[1, i, 1] = SaveData.ScaleBeat1[i];
            }
            if (i < SaveData.ScaleBeat2.Length)
            {
                arr_orig[2, i, 1] = SaveData.ScaleBeat2[i];
            }
            if (i < SaveData.ScaleBeat3.Length)
            {
                arr_orig[3, i, 1] = SaveData.ScaleBeat3[i];
            }
            if (i < SaveData.ScaleBeat4.Length)
            {
                arr_orig[4, i, 1] = SaveData.ScaleBeat4[i];
            }
            if (i < SaveData.ScaleBeat5.Length)
            {
                arr_orig[5, i, 1] = SaveData.ScaleBeat5[i];
            }
            if (i < SaveData.ScaleBeat6.Length)
            {
                arr_orig[6, i, 1] = SaveData.ScaleBeat6[i];
            }
            if (i < SaveData.ScaleBeat7.Length)
            {
                arr_orig[7, i, 1] = SaveData.ScaleBeat7[i];
            }
            if (i < SaveData.ScaleBeat8.Length)
            {
                arr_orig[8, i, 1] = SaveData.ScaleBeat8[i];
            }
            if (i < SaveData.ScaleBeat9.Length)
            {
                arr_orig[9, i, 1] = SaveData.ScaleBeat9[i];
            }
            if (i < SaveData.ScaleBeat10.Length)
            {
                arr_orig[10, i, 1] = SaveData.ScaleBeat10[i];
            }
            if (i < SaveData.ScaleBeat11.Length)
            {
                arr_orig[11, i, 1] = SaveData.ScaleBeat11[i];
            }
            if (i < SaveData.ScaleBeat12.Length)
            {
                arr_orig[12, i, 1] = SaveData.ScaleBeat12[i];
            }
            if (i < SaveData.ScaleBeat13.Length)
            {
                arr_orig[13, i, 1] = SaveData.ScaleBeat13[i];
            }
            if (i < SaveData.ScaleBeat14.Length)
            {
                arr_orig[14, i, 1] = SaveData.ScaleBeat14[i];
            }
            if (i < SaveData.ScaleBeat15.Length)
            {
                arr_orig[15, i, 1] = SaveData.ScaleBeat15[i];
            }
            if (i < SaveData.ScaleBeat16.Length)
            {
                arr_orig[16, i, 1] = SaveData.ScaleBeat16[i];
            }
            if (i < SaveData.ScaleBeat17.Length)
            {
                arr_orig[17, i, 1] = SaveData.ScaleBeat17[i];
            }
            if (i < SaveData.ScaleBeat18.Length)
            {
                arr_orig[18, i, 1] = SaveData.ScaleBeat18[i];
            }
            if (i < SaveData.ScaleBeat19.Length)
            {
                arr_orig[19, i, 1] = SaveData.ScaleBeat19[i];
            }
            if (i < SaveData.ScaleBeat20.Length)
            {
                arr_orig[20, i, 1] = SaveData.ScaleBeat20[i];
            }
            if (i < SaveData.ScaleBeat21.Length)
            {
                arr_orig[21, i, 1] = SaveData.ScaleBeat21[i];
            }
            if (i < SaveData.ScaleBeat22.Length)
            {
                arr_orig[22, i, 1] = SaveData.ScaleBeat22[i];
            }
            if (i < SaveData.ScaleBeat23.Length)
            {
                arr_orig[23, i, 1] = SaveData.ScaleBeat23[i];
            }
            if (i < SaveData.ScaleBeat24.Length)
            {
                arr_orig[24, i, 1] = SaveData.ScaleBeat24[i];
            }
            if (i < SaveData.ScaleBeat25.Length)
            {
                arr_orig[25, i, 1] = SaveData.ScaleBeat25[i];
            }
            if (i < SaveData.ScaleBeat26.Length)
            {
                arr_orig[26, i, 1] = SaveData.ScaleBeat26[i];
            }

            /*
            arr_orig[4, i, 0] = SaveData.ScaleNote4[i];
            arr_orig[5, i, 0] = SaveData.ScaleNote5[i];
            arr_orig[6, i, 0] = SaveData.ScaleNote6[i];
            arr_orig[7, i, 0] = SaveData.ScaleNote7[i];
            arr_orig[8, i, 0] = SaveData.ScaleNote8[i];
            arr_orig[9, i, 0] = SaveData.ScaleNote9[i];
            arr_orig[10, i, 0] = SaveData.ScaleNote10[i];
            arr_orig[11, i, 0] = SaveData.ScaleNote11[i];
            arr_orig[12, i, 0] = SaveData.ScaleNote12[i];
            arr_orig[13, i, 0] = SaveData.ScaleNote13[i];
            arr_orig[14, i, 0] = SaveData.ScaleNote14[i];
            arr_orig[15, i, 0] = SaveData.ScaleNote15[i];
            arr_orig[16, i, 0] = SaveData.ScaleNote16[i];
            arr_orig[17, i, 0] = SaveData.ScaleNote17[i];
            arr_orig[18, i, 0] = SaveData.ScaleNote18[i];
            arr_orig[19, i, 0] = SaveData.ScaleNote19[i];
            arr_orig[20, i, 0] = SaveData.ScaleNote20[i];
            arr_orig[21, i, 0] = SaveData.ScaleNote21[i];
            arr_orig[22, i, 0] = SaveData.ScaleNote22[i];
            arr_orig[23, i, 0] = SaveData.ScaleNote23[i];
            arr_orig[24, i, 0] = SaveData.ScaleNote24[i];
            arr_orig[25, i, 0] = SaveData.ScaleNote25[i];
            arr_orig[26, i, 0] = SaveData.ScaleNote26[i];

            arr_orig[0, i, 1] = SaveData.ScaleBeat0[i];
            arr_orig[1, i, 1] = SaveData.ScaleBeat1[i];
            arr_orig[2, i, 1] = SaveData.ScaleBeat2[i];
            arr_orig[3, i, 1] = SaveData.ScaleBeat3[i];
            arr_orig[4, i, 1] = SaveData.ScaleBeat4[i];
            arr_orig[5, i, 1] = SaveData.ScaleBeat5[i];
            arr_orig[6, i, 1] = SaveData.ScaleBeat6[i];
            arr_orig[7, i, 1] = SaveData.ScaleBeat7[i];
            arr_orig[8, i, 1] = SaveData.ScaleBeat8[i];
            arr_orig[9, i, 1] = SaveData.ScaleBeat9[i];
            arr_orig[10, i, 1] = SaveData.ScaleBeat10[i];
            arr_orig[11, i, 1] = SaveData.ScaleBeat11[i];
            arr_orig[12, i, 1] = SaveData.ScaleBeat12[i];
            arr_orig[13, i, 1] = SaveData.ScaleBeat13[i];
            arr_orig[14, i, 1] = SaveData.ScaleBeat14[i];
            arr_orig[15, i, 1] = SaveData.ScaleBeat15[i];
            arr_orig[16, i, 1] = SaveData.ScaleBeat16[i];
            arr_orig[17, i, 1] = SaveData.ScaleBeat17[i];
            arr_orig[18, i, 1] = SaveData.ScaleBeat18[i];
            arr_orig[19, i, 1] = SaveData.ScaleBeat19[i];
            arr_orig[20, i, 1] = SaveData.ScaleBeat20[i];
            arr_orig[21, i, 1] = SaveData.ScaleBeat21[i];
            arr_orig[22, i, 1] = SaveData.ScaleBeat22[i];
            arr_orig[23, i, 1] = SaveData.ScaleBeat23[i];
            arr_orig[24, i, 1] = SaveData.ScaleBeat24[i];
            arr_orig[25, i, 1] = SaveData.ScaleBeat25[i];
            arr_orig[26, i, 1] = SaveData.ScaleBeat26[i];
*/
        }

        for (int m = 0; m < scl_num; m++)
        {
            input_scale_name[m] = SaveData.ScaleName[m];
            scale_pos[m] = SaveData.ScalePos[m];
        }
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {

            make_scale_button();
        }
    }




    public static void make_scale_button()
    {
        int div = 0;
        int rem = 0;
        Text grandChild;
        GameObject NewScale;
        GameObject[] SclBtn_ = new GameObject[scl_num];

        for (int x = 0; x < scl_num; x++)
        {

            div = Math.DivRem(scale_pos[x], 9, out rem);　　//scale_posx から順番に配置をしていく。ボタンの名前は変わっていない。よって対応のスケールも変わらないはず


            NewScale = Instantiate(Resources.Load("Materials/CreScl/SclBtn") as GameObject, new Vector3(-900 + 180 * (rem + 1), 170 + 100 * (div + 1), 0), Quaternion.identity);
            NewScale.name = "SclBtn_" + x;
            GameObject canvas = GameObject.Find("CanvasBtn");
            grandChild = NewScale.GetComponentInChildren<Text>();
            grandChild.text = input_scale_name[x];
            NewScale.transform.SetParent(canvas.transform, false);
            Button SclBtn_0 = GameObject.Find("SclBtn_0").GetComponent<Button>();

            SclBtn_0.onClick.Invoke();//ボタンが何も選択しないでスタートすると、バグが起きる（キーが青になる。最低音にボタンをかざすと最高音も青になる）
        }
    }

}
