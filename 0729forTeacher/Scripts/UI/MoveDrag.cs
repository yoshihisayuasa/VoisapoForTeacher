using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
public class MoveDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
, IDropHandler
{
    GameObject slctd_Btn = null;
    GameObject toCkdBtn;
    int to_pos = 0;
    int id = 0;
    int to_id = 0;
    public static int PosDelBtn = 8;
    int[] PosCash = new int[27];
    GameObject DrangdBtn;
    Vector3 InitialTransform;
    // ドラックが開始したとき呼ばれる.



    public void OnBeginDrag(PointerEventData data)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = false;


        InitialTransform = data.pointerDrag.transform.position;
        DrangdBtn = data.pointerDrag.gameObject;
        data.pointerDrag.transform.SetAsLastSibling();


    }

    // ドラック中に呼ばれる.
    public void OnDrag(PointerEventData data)
    {

        Vector3 TargetPos = Camera.main.ScreenToWorldPoint(data.position);
        transform.position = GetLocalPosition(data.position, this.transform) * 0.00042f; //CamvasBtnのスケールをかける
    }

    private static Vector3 GetLocalPosition(Vector3 position, Transform transform)
    {
        // 画面上の座標 (Screen Point) を RectTransform 上のローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            position,
            Camera.main,
            out var result);
        return new Vector3(result.x, result.y, 0);
    }

    // ドラックが終了したとき呼ばれる.

    public void OnEndDrag(PointerEventData data)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        data.pointerDrag.transform.position = InitialTransform;

    }

    public void OnDrop(PointerEventData data)
    {

        bool noduplication = true;
        bool notoverflowed = true;
        toCkdBtn = this.gameObject;
        slctd_Btn = data.pointerDrag;
        int NoduplicationID;
        id = int.Parse(Regex.Replace(slctd_Btn.name, @"[^0-9^]", ""));
        to_id = int.Parse(Regex.Replace(toCkdBtn.name, @"[^0-9^]", ""));
        to_pos = ControlScale.scale_pos[to_id]; //2 
        NoduplicationID = id;

        JSON SaveData = Save.loadPlayerData();

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
                    if (PosCash[s] == PosCash[NoduplicationID])  //ドロップした番号と一緒のポジションがあったら1をたす。
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

        Save.savearray(SaveData, false);




        for (int i = 0; i < ControlScale.scl_num; i++)
        {
            GameObject Obj = GameObject.Find("SclBtn_" + i);
            if (Obj != null)
            {
                Destroy(Obj);

            }
        }

        ControlScale.make_scale_button();
        slctd_Btn = null;
        toCkdBtn = null;

        Debug.Log("OnDrop" + to_pos);


    }
}

