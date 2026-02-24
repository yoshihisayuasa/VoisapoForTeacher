using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
public class PosHolder : MonoBehaviour, IDropHandler
{
    // Start is called before the first frame update

    GameObject slctd_Btn = null;
    GameObject toCkdBtn;
    int to_pos = 0;
    int id = 0;
    // int to_id = 0;
    int[] PosCash = new int[27];
    int PosDelBtn = 8;


    public void OnDrop(PointerEventData data)
    {


        JSON SaveData = Save.loadPlayerData();

        toCkdBtn = this.gameObject;
        slctd_Btn = data.pointerDrag;
        bool noduplication = true;
        int NoduplicationID;



        bool notoverflowed = true;

        id = int.Parse(Regex.Replace(slctd_Btn.name, @"[^0-9^]", ""));


        to_pos = int.Parse(Regex.Replace(toCkdBtn.name, @"[^0-9^]", ""));

        NoduplicationID = id;



        for (int s = 0; s < ControlScale.scl_num; s++)
        {
            PosCash[s] = ControlScale.scale_pos[s];
        }

        PosCash[id] = to_pos;




        while (true)
        {
            noduplication = true;
            for (int s = 0; s < ControlScale.scl_num; s++)
            {
                if (NoduplicationID != s)
                {
                    if (PosCash[s] == PosCash[NoduplicationID])
                    {
                        PosCash[s] = PosCash[s] + 1;
                        NoduplicationID = s;

                        if (PosCash[s] == PosDelBtn) //pos10は使用不可
                        {
                            PosCash[s] = PosDelBtn + 1;
                        }
                        noduplication = false;

                    }
                }
            }
            if (noduplication)
            {
                break;
            }
        }



        for (int s = 0; s < ControlScale.scl_num; s++)
        {
            if (PosCash[s] > (PosCash.Length - 1))
            {
                notoverflowed = false;
            }
        }


        if (notoverflowed)
        {

            for (int s = 0; s < ControlScale.scl_num; s++)
            {
                ControlScale.scale_pos[s] = PosCash[s];
            }
        }



        for (int i = 0; i < ControlScale.scl_num; i++)
        {
            GameObject Obj = GameObject.Find("SclBtn_" + i);

            if (Obj != null)
            {
                Destroy(Obj);

            }
        }

        Save.savearray(SaveData, false);
        ControlScale.make_scale_button();
        slctd_Btn = null;
        toCkdBtn = null;

    }
}

