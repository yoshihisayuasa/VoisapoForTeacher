using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class BPM : MonoBehaviourPunCallbacks
{

    //マッチング前に先生側が値を変えていた場合にも生徒側と値がずれないようにした。
    //配列でポインタ参照にして増減地だけでなく、ボタン起動時毎回値を渡すようにした。しかしうまく動かない。
    //マッチングした時点でBPMをリセットする仕様にする。でないと+-ボタンをクリックしないとBPMの値が反映されないため結局キー入力時毎回BPMを更新しないといけなくなる。無駄。

    // Image BPM_Gage;
    string ColorManager;

    public Text bmpText; // スコアの UI
    public static float bpm = 180.0f;
    float time = 0;
    //Color black = new Color32(33, 36, 41, 0);
    Color water = new Color32(0, 156, 255, 255);

    void Start()
    {
        bmpText.text = bpm.ToString() + "BPM";  //これがないとシーン遷移時BPMがリセットされる
        ColorManager = "black";

    }



    public void bpm_rpc_positive()
    {
        StopCoroutine("flashing");
        time = 0;

        StartCoroutine("flashing");

        bpm = bpm + 10;
        bmpText.text = bpm.ToString() + "BPM";

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(bpm_match), RpcTarget.Others, bpm);
        }

    }
    public void bpm_rpc_negative()
    {
        StopCoroutine("flashing");
        time = 0;
        StartCoroutine("flashing");

        bpm = bpm - 10;
        bmpText.text = bpm.ToString() + "BPM";

        if (MatchmakingView.roomenterdflag)
        {
            photonView.RPC(nameof(bpm_match), RpcTarget.Others, bpm);
        }
    }


    public void bpm_rpc_match()
    {
        photonView.RPC(nameof(bpm_match), RpcTarget.Others, bpm);
    }




    [PunRPC]
    public void bpm_match(float bpm) //マッチ時に同期させる処理
    {

    }

    public static float bpm_time()
    {
        return (60.0f / bpm);
    }


    private IEnumerator flashing()
    {
        while (time < 6.0f)
        {

            if (ColorManager != "blue")
            {
                bmpText.color = water;
                ColorManager = "blue";
            }
            else
            {
                bmpText.color = Color.white;
                ColorManager = "white";
            }
            yield return WaitForSecondsCache.Wait(BPM.bpm_time() / 2);

            time += BPM.bpm_time() / 2;

        }
        StopCoroutine("flashing");
        bmpText.color = Color.white;
        ColorManager = "white";

    }
}