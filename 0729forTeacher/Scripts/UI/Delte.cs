using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
public class Delte : MonoBehaviour, IDropHandler
{
    int id = 0;



    public void OnDrop(PointerEventData data)
    {
        JSON SaveData = Save.loadPlayerData();
        GameObject Btn = data.pointerDrag;




        if ((Btn.name != "SclBtn_0") && (Btn.name != "SclBtn_1"))
        {

            id = Int32.Parse(Btn.name.Remove(0, 7));




            String BtnName = Btn.transform.GetChild(0).gameObject.GetComponent<Text>().text;

            for (int i = 0; i < ControlScale.scl_num; i++)
            {
                GameObject Obj = GameObject.Find("SclBtn_" + i);

                if (i > id)
                {
                    for (int r = 0; r < ControlScale.KeyNum; r++)   //スケールを消えた分をずらす処理
                    {
                        ControlScale.arr_orig[i - 1, r, 0] = ControlScale.arr_orig[i, r, 0];
                        ControlScale.arr_orig[i - 1, r, 1] = ControlScale.arr_orig[i, r, 1];

                    }

                    ControlScale.scale_pos[i - 1] = ControlScale.scale_pos[i];
                    ControlScale.input_scale_name[i - 1] = ControlScale.input_scale_name[i];
                }

                if (Obj != null)
                {
                    Destroy(Obj);
                }
            }

            ControlScale.scl_num = ControlScale.scl_num - 1;


            //  Save.savePlayerData(SaveData);

            for (int i = 0; i < 27; i++)
            {
                SaveData.ScaleName[i] = ControlScale.input_scale_name[i];
                SaveData.ScalePos[i] = ControlScale.scale_pos[i]; ;
            }

            //SaveData.ScaleNum = ControlScale.scl_num;
            Save.savearray(SaveData, true);
            // Save.saveScalenum(SaveData);
            ControlScale.make_scale_button();

        }

    }



}



