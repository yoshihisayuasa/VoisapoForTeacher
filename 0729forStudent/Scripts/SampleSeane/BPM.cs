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
    //BPMはプラスマイナスするのは無駄、値が何らかの原因で会わなくなったときに永遠に補正されない。毎回絶対値を送信する。
    //生徒が非アクティブのときにBPM変更して、その後クリックしなかったらどうなるんだろう。それは可能なのか？可能だった。自動移調時、復帰したら再生が再開される

    static float bpm = 180.0f;

    [PunRPC]
    public void bpm_match(float curr_bpm) //マッチ時に同期させる処理
    {
        bpm = curr_bpm;
    }

    public static float bpm_time()
    {
        return (60.0f / bpm);
    }


}

