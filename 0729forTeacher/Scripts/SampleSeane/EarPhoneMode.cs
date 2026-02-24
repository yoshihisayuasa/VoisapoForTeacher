using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class EarPhoneMode : MonoBehaviourPunCallbacks
{

    public static bool EarPhoneModeBool = false;
    public GameObject EarPhoneModeBtnObject;
    public static Image EarPhoneModeBool_Sprite;

    void Start()
    {
        EarPhoneModeBool_Sprite = EarPhoneModeBtnObject.GetComponent<Image>();

        if (EarPhoneModeBool)
        {
            EarPhoneModeBool_Sprite.color = Color.red;
        }
        else
        {
            EarPhoneModeBool_Sprite.color = Color.white;
        }

    }
    public void EarPhoneModeBtn()
    {
        if (EarPhoneModeBool)
        {
            EarPhoneModeBool = false;
            EarPhoneModeBool_Sprite.color = Color.white;
            RPCEarPhoneModeMatch();

        }
        else if (MatchmakingView.roomenterdflag)
        {

            EarPhoneModeBool = true;

            EarPhoneModeBool_Sprite.color = Color.red;
            RPCEarPhoneModeMatch();

        }

    }

    public void RPCEarPhoneModeMatch()
    {
        photonView.RPC(nameof(EarPhoneModeMatch), RpcTarget.Others, EarPhoneModeBool);
    }




    [PunRPC]
    public void EarPhoneModeMatch(float EarPhoneModeBool) //マッチ時に同期させる処理
    {

    }





}
